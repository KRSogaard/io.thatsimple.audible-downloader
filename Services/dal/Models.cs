using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudibleDownloader.Services.dal
{

    public class BulkAudibleBook
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("released")]
        public int Released { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("lastUpdated")]
        public long LastUpdated { get; set; }
        
        [JsonPropertyName("series")]
        public List<SimpleSeries> Series { get; set; }

        [JsonPropertyName("authors")]
        public List<IdValueInfo> Authors { get; set; }

        [JsonPropertyName("tags")]
        public List<IdValueInfo> Tags { get; set; }

        [JsonPropertyName("narrators")]
        public List<IdValueInfo> Narrators { get; set; }

        [JsonPropertyName("categories")]
        public List<IdValueInfo> Categories { get; set; }

    }

    public class IdValueInfo
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

    }

    public class AudibleBook
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("length")]
        public int? Length { get; set; }

        [JsonPropertyName("released")]
        public long? Released { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("lastUpdated")]
        public long? LastUpdated { get; set; }

        [JsonPropertyName("series")]
        public List<SimpleSeries> Series { get; set; }

        [JsonPropertyName("authors")]
        public List<AudibleAuthor> Authors { get; set; }

        [JsonPropertyName("tags")]
        public List<AudibleTag> Tags { get; set; }

        [JsonPropertyName("narrators")]
        public List<AudibleNarrator> Narrators { get; set; }

        [JsonPropertyName("categories")]
        public List<AudibleCategory> Categories { get; set; }
        
        [JsonPropertyName("shouldDownload")]
        public bool ShouldDownload { get; set; }

    }

    public class SimpleSeries
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bookNumber")]
        public string? BookNumber { get; set; }

    }

    public class AudibleAuthor
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

    }

    public class AudibleNarrator
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

    }

    public class AudibleCategory
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

    }

    public class AudibleTag
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

    }

    public class AudibleSeries
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lastUpdated")]
        public long LastUpdated { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }
        
        [JsonPropertyName("shouldDownload")]
        public bool ShouldDownload { get; set; }

    }

}
