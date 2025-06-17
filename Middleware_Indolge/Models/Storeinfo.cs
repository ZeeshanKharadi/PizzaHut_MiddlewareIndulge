using Newtonsoft.Json;

namespace Middleware_Indolge.Models
{
    public class StoreInfoWrapper
    {
        [JsonProperty("value")]
        public List<StoreInfo> Value { get; set; }
    }

    public class StoreInfo
    {
        [JsonProperty("CustName")]
        public string CustName { get; set; }

        [JsonProperty("DTLoginUrl")]
        public string DTLoginUrl { get; set; }

        [JsonProperty("RetailChannelId")]
        public string RetailChannelId { get; set; }

        [JsonProperty("Company")]
        public string Company { get; set; }

        [JsonProperty("Store")]
        public string Store { get; set; }

        [JsonProperty("StoreName")]
        public string StoreName { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("DTCreateUrl")]
        public string DTCreateUrl { get; set; }
        [JsonProperty("RSSUUrl")]
        public string RSSUUrl { get; set; }
    }

}
