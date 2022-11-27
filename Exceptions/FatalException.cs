namespace AudibleDownloader.Exceptions;

public class FatalException : Exception
{
    public FatalException(string message) : base(message)
    {
    }

    public FatalException()
    {
    }
}