using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudibleDownloader.Utils
{
    public static class MapUtil
    {
        public static string CreateMapPart(object obj)
        {
            return ReplaceProtected(JsonSerializer.Serialize(obj)) + "|";
        }

        public static List<T> ParseMap<T>(string map)
        {
            if (String.IsNullOrWhiteSpace(map))
            {
                return new List<T>();
            }

            var parts = map.Split('|');
            List<T> result = new List<T>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(parts[i]))
                {
                    continue;
                }

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
}
