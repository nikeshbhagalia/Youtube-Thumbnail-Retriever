using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GetYoutubeThumbnail.Tests.Models
{
    public class YoutubePostRequestBody
    {
        [JsonProperty("context")]
        public JObject Context { get; set; }

        [JsonProperty("continuation")]
        public string Continuation { get; set; }
    }
}
