namespace UFormKit
{
    public class UFormSettings
    {
        public bool DisableTelemetry { get; set; }
        public RecaptchaSettings Recaptcha { get; set; }
    }

    public class RecaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
    }
}
