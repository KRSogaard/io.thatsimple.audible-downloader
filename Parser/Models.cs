using System.Text.Json.Serialization;

namespace AudibleDownloader.Parser;

public class ParseAudioBook
{
    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("asin")] public string Asin { get; set; }

    [JsonPropertyName("released")] public long Released { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("image")] public string Image { get; set; }

    [JsonPropertyName("subtitle")] public string Subtitle { get; set; }

    [JsonPropertyName("authors")] public List<ParseAudioBookPerson> Authors { get; set; }

    [JsonPropertyName("narrators")] public List<string> Narrators { get; set; }

    [JsonPropertyName("runtime")] public int RuntimeSeconds { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }

    [JsonPropertyName("series")] public List<ParseAudioBookSeries> Series { get; set; }

    [JsonPropertyName("categories")] public List<ParseAudioBookCategory> Categories { get; set; }

    [JsonPropertyName("tags")] public List<string> Tags { get; set; }
}

public class ParseAudioBookPerson
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("asin")] public string Asin { get; set; }
}

public class ParseAudioBookSeries
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("asin")] public string Asin { get; set; }

    [JsonPropertyName("bookNumber")] public string? BookNumber { get; set; }

    [JsonPropertyName("bookNumber")] public int? Sort { get; set; }

    [JsonPropertyName("summary")] public string? Summary { get; set; }
}

public class ParseAudioBookCategory
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }
}

public class ParseSeries
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("asin")] public string Asin { get; set; }

    [JsonPropertyName("books")] public List<ParseSeriesBook> Books { get; set; }
}

public class ParseSeriesBook
{

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("asin")] public string Asin { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }
    [JsonPropertyName("bookNumber")] public string? BookNumber { get; set; }
    [JsonPropertyName("sort")] public int? Sort { get; set; }

    [JsonPropertyName("releaseDate")] public long? ReleaseDate { get; set; }

    [JsonPropertyName("lengthSeconds")] public int? LengthSeconds { get; set; }
}