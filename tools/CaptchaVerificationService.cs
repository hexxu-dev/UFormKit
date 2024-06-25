using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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
    }
}
