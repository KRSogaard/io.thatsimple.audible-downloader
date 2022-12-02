using System.Text;

namespace AudibleDownloader.Utils;

public static class StringStreams
{
    public static Stream ToStream(this string value) => ToStream(value, Encoding.UTF8);

    public static Stream ToStream(this string value, Encoding encoding) 
        => new MemoryStream(encoding.GetBytes(value ?? string.Empty));
}