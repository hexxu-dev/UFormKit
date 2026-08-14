using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using UFormKit.Helpers;
using UFormKit.Notifications;

namespace UFormKit;

public class UFormComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<ContentSavedNotification, UFormContentSavedNotificationHandler>();
        builder.Services.AddTransient<CaptchaVerificationService>();
        builder.Services.Configure<UFormSettings>(builder.Config.GetSection("UFormKit"));
    }
}
