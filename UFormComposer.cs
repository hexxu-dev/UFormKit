using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using UFormKit.Helpers;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace UFormKit
{
    public class UFormComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<SendingContentNotification, UFormSendingContentNotificationHandler>();
            builder.AddNotificationAsyncHandler<ContentSavedNotification, UFormContentSavedNotificationHandler>();
            builder.Services.AddTransient<CaptchaVerificationService>();
            builder.Services.Configure<UFormSettings>(builder.Config.GetSection("UFormKit"));
        }
    }
}
