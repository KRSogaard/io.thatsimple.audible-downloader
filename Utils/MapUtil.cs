using System.Text.Json;

namespace AudibleDownloader.Utils;

public static class MapUtil
{
    public static string CreateMapPart(object obj)
    {
        return ReplaceProtected(JsonSerializer.Serialize(obj)) + "|";
    }

    public static List<T> ParseMap<T>(string map)
    {
        if (string.IsNullOrWhiteSpace(map)) return new List<T>();

        var parts = map.Split('|');
        var result = new List<T>();
        for (var i = 0; i < parts.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(parts[i])) continue;

            result.Add(JsonSerializer.Deserialize<T>(parts[i]));
        }

        return result;
    }

    private static string ReplaceBack(string v)
    {
        return v.Replace("!&!", "|");
    }

    private static string ReplaceProtected(string v)
    {
        return v.Replace("|", "!&!");
    }
}