using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UFormKit.Models
{
    public class CaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }


        public string Hostname { get; set; }


        [JsonProperty("error-codes")]
        public string[] Errorcodes { get; set; }
    }

    public class CftsVerificationResponse
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}
