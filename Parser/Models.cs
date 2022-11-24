using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudibleDownloader.Parser
{

    public class ParseAudioBook
    {

        [JsonPropertyName("title")]
        public string Title;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("released")]
        public int Released;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("image")]
        public string Image;

        [JsonPropertyName("subtitle")]
        public string Subtitle;

        [JsonPropertyName("authors")]
        public List<ParseAudioBookPerson> Authors;

        [JsonPropertyName("narrators")]
        public List<string> Narrators;

        [JsonPropertyName("runtime")]
        public int Runtime;

        [JsonPropertyName("summary")]
        public string Summary;
        
        [JsonPropertyName("series")]
        public List<ParseAudioBookSeries> Series;

        [JsonPropertyName("categories")]
        public List<ParseAudioBookCategory> Categories;

        [JsonPropertyName("tags")]
        public List<string> Tags;

    }

    public class ParseAudioBookPerson
    {

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("asin")]
        public string Asin;

    }

    public class ParseAudioBookSeries
    {

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("bookNumber")]
        public string? BookNumber;

        [JsonPropertyName("summary")]
        public string? Summary;

    }
    
    public class ParseAudioBookCategory
    {

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("id")]
        public string Id;

    }

    public class ParseSeries
    {

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("summary")]
        public string Summary;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("books")]
        public List<ParseSeriesBook> Books;

    }

    public class ParseSeriesBook
    {

        [JsonPropertyName("bookNumber")]
        public string BookNumber;

        [JsonPropertyName("title")]
        public string Title;

        [JsonPropertyName("asin")]
        public string Asin;

        [JsonPropertyName("link")]
        public string Link;

        [JsonPropertyName("releaseDate")]
        public int ReleaseDate;

        [JsonPropertyName("lengthSeconds")]
        public int LengthSeconds;

    }

}
