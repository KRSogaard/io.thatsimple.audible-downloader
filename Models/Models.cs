using System.Text.Json.Serialization;

namespace AudibleDownloader.Models;

public class IdValueInfo {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public class AudibleBook {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asin")]
    public string Asin { get; set; }

    [JsonPropertyName("asin")]
    public long? Isbn { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    [JsonPropertyName("released")]
    public long? Released { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("lastUpdated")]
    public long LastUpdated { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("shouldDownload")]
    public bool ShouldDownload { get; set; }

    [JsonPropertyName("isTemp")]
    public bool IsTemp { get; set; }
}

public class AudibleAuthor {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asin")]
    public string Asin { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }
}

public class AudiblePublisher {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }
}

public class AudibleNarrator {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }
}

public class AudibleCategory {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }
}

public class AudibleTag {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }
}

public class AudibleSeries {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asin")]
    public string Asin { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("lastUpdated")]
    public long LastUpdated { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("shouldDownload")]
    public bool ShouldDownload { get; set; }

    [JsonPropertyName("lastChecked")]
    public long? LastChecked { get; set; }
}