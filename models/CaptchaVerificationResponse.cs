using Newtonsoft.Json;
using System;

namespace UFormKit.Models
{
    public class CaptchaVerificationResponse
    {
        public bool Success { get; set; }


        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }


        public string Hostname { get; set; }


        [JsonProperty("error-codes")]
        public string[] Errorcodes { get; set; }
    }
}
