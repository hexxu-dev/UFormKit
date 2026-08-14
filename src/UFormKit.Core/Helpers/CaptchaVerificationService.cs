using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UFormKit.Models;

namespace UFormKit.Helpers
{
    public class CaptchaVerificationService
    {
        private ILogger<CaptchaVerificationService> logger;
        private UFormSettings uformSettings;

        public CaptchaVerificationService(IOptions<UFormSettings> uformSettings)
        {
            this.uformSettings = uformSettings.Value;
        }

        public async Task<bool> IsCaptchaValid(string token)
        {
            var result = false;

            var googleVerificationUrl = "https://www.google.com/recaptcha/api/siteverify";

            try
            {
                using var client = new HttpClient();
                if (uformSettings.Recaptcha != null)
                {
                    var secretKey = uformSettings.Recaptcha.SecretKey;

                    var response = await client.PostAsync($"{googleVerificationUrl}?secret={secretKey}&response={token}", null);
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var captchaVerfication = JsonConvert.DeserializeObject<CaptchaVerificationResponse>(jsonString);

                    var scoreThreshold = uformSettings.Recaptcha.ScoreThreshold > 0 ? uformSettings.Recaptcha.ScoreThreshold : 0.5;
                    result = captchaVerfication.Success && captchaVerfication.Score >= scoreThreshold;
                }
            }
            catch (Exception e)
            {
                logger.LogError("Failed to process captcha validation", e);
            }

            return result;
        }


        public async Task<bool> IsCftsValid(string token)
        {
            var result = false;

            var verificationUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            var secretKey = uformSettings.CloudflareTurnstile.SecretKey;

            try
            {
                using var client = new HttpClient();
                var formData = new Dictionary<string, string>
                    {
                        { "secret", secretKey },
                        { "response", token }
                    };
                var content = new FormUrlEncodedContent(formData);
                var response = await client.PostAsync(verificationUrl, content);
                var jsonString = await response.Content.ReadAsStringAsync();
                var captchaVerification = JsonConvert.DeserializeObject<CftsVerificationResponse>(jsonString);

                result = captchaVerification?.Success ?? false;

            }
            catch (Exception e)
            {
                logger.LogError("Failed to process cfts validation", e);
            }

            return result;
        }
    }
}
