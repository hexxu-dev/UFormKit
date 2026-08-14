using Microsoft.AspNetCore.Html;

namespace UFormKit.Models
{
    public class UFormModel
    {
        public HtmlString Template { get; set; }
        public bool UseRecaptcha { get; set; }
        public string SiteKey { get; set; }
        public string CftsKey { get; set; }
        public bool UseCfts { get; set; }
    }

}
