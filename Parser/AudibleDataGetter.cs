namespace AudibleDownloader.Parser;

public interface AudibleDataGetter {
    public Task<ParseAudioBook> ParseBook(string asin);
    public Task<ParseSeries> ParseSeries(string asin);
}