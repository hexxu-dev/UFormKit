using System.Net.Http.Json;
using System.Reflection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace UFormKit.Notifications;

public class UFormContentSavedNotificationHandler : INotificationAsyncHandler<ContentSavedNotification>
{
    private readonly GlobalSettings _globalSettings;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UFormSettings _uformSettings;

    public UFormContentSavedNotificationHandler(
        IOptions<GlobalSettings> globalSettings,
        IUmbracoVersion umbracoVersion,
        IHttpClientFactory httpClientFactory,
        IOptions<UFormSettings> uformSettings)
    {
        _globalSettings = globalSettings.Value;
        _umbracoVersion = umbracoVersion;
        _httpClientFactory = httpClientFactory;
        _uformSettings = uformSettings.Value;
    }

    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken)
    {
        if (_uformSettings.DisableTelemetry)
        {
            return;
        }

        var assembly = Assembly.GetAssembly(typeof(UFormContentSavedNotificationHandler))!;
        var packageVersion = SemVersion.Parse(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion).ToSemanticStringWithoutBuild();
        var packageName = assembly.GetName().Name;

        foreach (var node in notification.SavedEntities)
        {
            if (!node.ContentType.Alias.Equals("uFormHexxu"))
            {
                continue;
            }

            var dirty = (IRememberBeingDirty)node;
            if (!dirty.WasPropertyDirty("Id"))
            {
                continue;
            }

            try
            {
                var umbracoId = Guid.TryParse(_globalSettings.Id, out var telemetrySiteIdentifier)
                    ? telemetrySiteIdentifier
                    : Guid.Empty;

                var data = new
                {
                    umbraco_id = umbracoId,
                    umbraco_version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                    package_id = packageName,
                    package_version = packageVersion,
                };

                var address = new Uri("https://telemetry.hexxu.dev/usage/insert");
                using var client = _httpClientFactory.CreateClient();
                using var post = await client.PostAsJsonAsync(address, data, cancellationToken);
            }
            catch
            {
                // telemetry must not affect content save
            }
        }
    }
}
