using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudibleDownloader.Services.dal
{

    public class BulkAudibleBook
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("title")]
        public string Title;

        [JsonPropertyName("length")]
        public int Length;

        [JsonPropertyName("released")]
        public int Released;

        [JsonPropertyName("summary")]
        public string Summary;

        [JsonPropertyName("lastUpdated")]
        public long LastUpdated;
        
        [JsonPropertyName("series")]
        public List<SimpleSeries> Series;

        [JsonPropertyName("authors")]
        public List<IdValueInfo> Authors;

        [JsonPropertyName("tags")]
        public List<IdValueInfo> Tags;

        [JsonPropertyName("narrators")]
        public List<IdValueInfo> Narrators;

        [JsonPropertyName("categories")]
        public List<IdValueInfo> Categories;

    }

    public class IdValueInfo
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("value")]
        public string Value;

    }

    public class AudibleBook
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("title")]
        public string Title;

        [JsonPropertyName("length")]
        public int Length;

        [JsonPropertyName("released")]
        public long Released;

        [JsonPropertyName("summary")]
        public string Summary;

        [JsonPropertyName("lastUpdated")]
        public long LastUpdated;

        [JsonPropertyName("series")]
        public List<SimpleSeries> Series;

        [JsonPropertyName("authors")]
        public List<AudibleAuthor> Authors;

        [JsonPropertyName("tags")]
        public List<AudibleTag> Tags;

        [JsonPropertyName("narrators")]
        public List<AudibleNarrator> Narrators;

        [JsonPropertyName("categories")]
        public List<AudibleCategory> Categories;

    }

    public class SimpleSeries
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("bookNumber")]
        public string BookNumber;

    }

    public class AudibleAuthor
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("created")]
        public long Created;

    }

    public class AudibleNarrator
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("created")]
        public long Created;

    }

    public class AudibleCategory
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("created")]
        public long Created;

    }

    public class AudibleTag
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("tag")]
        public string Tag;

        [JsonPropertyName("created")]
        public long Created;

    }

    public class AudibleSeries
    {

        [JsonPropertyName("id")]
        public int Id;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("summary")]
        public string Summary;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("lastUpdated")]
        public long LastUpdated;

        [JsonPropertyName("created")]
        public long Created;

    }

}
