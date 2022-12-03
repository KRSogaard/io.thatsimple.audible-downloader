using System.Text;

namespace AudibleDownloader.Utils;

public static class StringStreams {
    public static Stream ToStream(this string value) {
        return ToStream(value, Encoding.UTF8);
    }

    public static Stream ToStream(this string value, Encoding encoding) {
        return new MemoryStream(encoding.GetBytes(value ?? string.Empty));
    }
}