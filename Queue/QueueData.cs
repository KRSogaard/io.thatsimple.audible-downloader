using System.Text.Json.Serialization;

namespace AudibleDownloader.Queue;

public class SeriesData
{
    [JsonPropertyName("asin")] public string Asin { get; set; }
}

public class BookData
{

    [JsonPropertyName("asin")] public string Asin { get; set; }
}

public class MessageData
{
    [JsonPropertyName("asin")] public string Asin { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("jobId")] public int? JobId { get; set; }

    [JsonPropertyName("userId")] public string? UserId { get; set; }

    [JsonPropertyName("addToUser")] public bool AddToUser { get; set; }

    [JsonPropertyName("force")] public bool Force { get; set; }
}