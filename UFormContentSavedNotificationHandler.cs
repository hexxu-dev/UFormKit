using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;
using static Lucene.Net.Search.FieldValueHitQueue;

namespace UFormKit
{
    public class UFormContentSavedNotificationHandler : INotificationAsyncHandler<ContentSavedNotification>
    {
        private  GlobalSettings globalSettings;
        private  IUmbracoVersion umbracoVersion;
        private  IHttpClientFactory httpClientFactory;
        private  UFormSettings uformSettings;

        public UFormContentSavedNotificationHandler(
            IOptions<GlobalSettings> globalSettings,
            IUmbracoVersion umbracoVersion, IHttpClientFactory httpClientFactory, IOptions<UFormSettings> uformSettings)
        {
            this.globalSettings = globalSettings.Value;
            this.umbracoVersion = umbracoVersion;
            this.httpClientFactory = httpClientFactory;
            this.uformSettings = uformSettings.Value;
        }


        async Task INotificationAsyncHandler<ContentSavedNotification>.HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken)
        {
            if (!uformSettings.DisableTelemetry) {
                var assembly = Assembly.GetAssembly(MethodBase.GetCurrentMethod().DeclaringType);
                var packageVersion = SemVersion.Parse(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion).ToSemanticStringWithoutBuild();
                var packageName = assembly.GetName().Name;

                foreach (var node in notification.SavedEntities)
            {
                    if (node.ContentType.Alias.Equals("uFormHexxu"))
                    {
                        var dirty = (IRememberBeingDirty)node;
                        var isNew = dirty.WasPropertyDirty("Id");
                        if (isNew)
                        {
                            try
                            {
                                var umbracoId = Guid.TryParse(globalSettings.Id, out var telemetrySiteIdentifier) == true
                               ? telemetrySiteIdentifier
                               : Guid.Empty;
                                
                                var data = new
                                {
                                    umbraco_id = umbracoId,
                                    umbraco_version = umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                                    package_id = packageName,
                                    package_version = packageVersion
                                };

                                var address = new Uri("https://telemetry.hexxu.dev/usage/insert");
                                using var client = httpClientFactory.CreateClient();
                                using var post = await client.PostAsJsonAsync(address, data);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}
