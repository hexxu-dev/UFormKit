using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UFormKit.notifications
{
    public class UFormSendingContentNotificationHandler : INotificationHandler<SendingContentNotification>
    {
        private GlobalSettings globalSettings;
        public UFormSendingContentNotificationHandler(IOptions<GlobalSettings> globalSettings)
        {
            this.globalSettings = globalSettings.Value;
        }
        public void Handle(SendingContentNotification notification)
        {
            if (notification.Content.ContentTypeAlias.Equals("uFormHexxu"))
            {
                foreach (var variant in notification.Content.Variants)
                {
                    if (variant.State == ContentSavedState.NotCreated)
                    {
                        var properties = variant.Tabs.SelectMany(f => f.Properties);
                        setDefaultValue(properties, "sendersMessageSuccess", "Thank you for your message. It has been sent.");
                        setDefaultValue(properties, "sendersMessageFailedToSend", "There was an error trying to send your message. Please try again later.");
                        setDefaultValue(properties, "validationErrorsOccurred", "One or more fields have an error. Please check and try again.");
                        setDefaultValue(properties, "thereAreTermsThatTheSenderMustAccept", "You must accept the terms and conditions before sending your message.");
                        setDefaultValue(properties, "thereIsAFieldThatTheSenderMustFillIn", "The field is required.");
                        setDefaultValue(properties, "thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength", "The field is too long.");
                        setDefaultValue(properties, "thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength", "The field is too short.");
                        setDefaultValue(properties, "uploadingAFileFailsForAnyReason", "There was an unknown error uploading the file.");
                        setDefaultValue(properties, "uploadedFileIsNotAllowedForFileType", "You are not allowed to upload files of this type.");
                        setDefaultValue(properties, "uploadedFileIsTooLarge", "The file is too big.");
                        setDefaultValue(properties, "dateFormatThatTheSenderEnteredIsInvalid", "The date format is incorrect.");
                        setDefaultValue(properties, "dateIsEarlierThanMinimumLimit", "The date is before the earliest one allowed.");
                        setDefaultValue(properties, "dateIsLaterThanMaximumLimit", "The date is after the latest one allowed.");
                        setDefaultValue(properties, "numberFormatThatTheSenderEnteredIsInvalid", "The number format is invalid.");
                        setDefaultValue(properties, "numberIsSmallerThanMinimumLimit", "The number is smaller than the minimum allowed.");
                        setDefaultValue(properties, "numberIsLargerThanMaximumLimit", "The number is larger than the maximum allowed.");
                        setDefaultValue(properties, "emailAddressThatTheSenderEnteredIsInvalid", "The e-mail address entered is invalid.");
                        setDefaultValue(properties, "uRLThatTheSenderEnteredIsInvalid", "The URL is invalid.");
                        setDefaultValue(properties, "telephoneNumberThatTheSenderEnteredIsInvalid", "The telephone number is invalid.");

                        var defaultTemplate = "<div> Your name\r\n    [text* your-name] </div>\r\n\r\n<div> Your email\r\n    [email* your-email] </div>\r\n\r\n<div> Subject\r\n    [text* your-subject] </div>\r\n\r\n<div> Your message (optional)\r\n    [textarea your-message] </div>\r\n\r\n[submit \"Submit\"]";
                        setDefaultValue(properties, "form", defaultTemplate);


                        if (globalSettings.Smtp != null)
                        {
                            setDefaultValue(properties, "from", globalSettings.Smtp.From);
                        }
                        setDefaultValue(properties, "subject", "[your-subject]");
                        setDefaultValue(properties, "replyTo", "[your-email]");
                        setDefaultValue(properties, "messageBody", "From: [your-name] [your-email]\r\nSubject: [your-subject]\r\n\r\nMessage Body:\r\n[your-message]\r\n");
                    }
                }
            }
        }

        private void setDefaultValue(IEnumerable<ContentPropertyDisplay> properties, string propertyName, string value)
        {
            var item = properties.FirstOrDefault(f => f.Alias.InvariantEquals(propertyName));
            if (item is not null)
            {
                item.Value = value;
            }
        }
    }
}
