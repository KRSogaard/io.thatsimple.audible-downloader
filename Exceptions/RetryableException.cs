namespace AudibleDownloader.Exceptions;

public class RetryableException : Exception {
    public RetryableException(string message) : base(message) { }

    public RetryableException() { }
}