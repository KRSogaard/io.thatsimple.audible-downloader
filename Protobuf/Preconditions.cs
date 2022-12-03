namespace AudibleDownloader.Protobuf;

public class Preconditions {
    public static T CheckNotNull<T>(T value, string name) where T : class {
        if (value == null) throw new ArgumentNullException("Parameter " + name + " was null");
        return value;
    }

    public static string CheckNotNullOrEmpty(string value, string name) {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("Parameter " + name + " was null or empty \"" + value + "\"");
        return value;
    }
}