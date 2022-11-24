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
        public string Name;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("asin")]
        public string Asin;
    }
    public class BookData
    {
        [JsonPropertyName("title")]
        public string Title;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("asin")]
        public string Asin;
    }
}
