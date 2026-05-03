using Newtonsoft.Json;

namespace HitokotoBetaExtension
{
    public class HitokotoItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("hitokoto")]
        public string Hitokoto { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("from_who")]
        public string FromWho { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }
}