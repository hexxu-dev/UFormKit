namespace UFormKit
    {
        public class UFormSettings
        {
            public bool DisableTelemetry { get; set; }
            public RecaptchaSettings Recaptcha { get; set; }
            public CloudflareTurnstileSettings CloudflareTurnstile { get; set; }
        }

        public class RecaptchaSettings
        {
            public string SiteKey { get; set; }
            public string SecretKey { get; set; }
            public double ScoreThreshold { get; set; }
        }

        public class CloudflareTurnstileSettings
        {
            public string SiteKey { get; set; }
            public string SecretKey { get; set; }
        }
    }

