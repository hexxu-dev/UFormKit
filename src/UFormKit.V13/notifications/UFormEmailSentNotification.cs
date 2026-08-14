using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Notifications;

namespace UFormKit.Notifications
{
    public class UFormEmailSentNotification : INotification
    {
        public IFormCollection Form { get; set; }
        public string Message { get; set; }

        public UFormEmailSentNotification(IFormCollection form, string message)
        {
            Form = form;
            Message = message;
        }
    }
}
