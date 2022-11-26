using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudibleDownloader.Queue
{
    public class SeriesData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }
    }
    public class BookData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }
    }

    public class MessageData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("jobId")]
        public int? JobId { get; set; }
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
        [JsonPropertyName("addToUser")]
        public bool AddToUser { get; set; }
        [JsonPropertyName("force")]
        public bool Force { get; set; }
    }
}
