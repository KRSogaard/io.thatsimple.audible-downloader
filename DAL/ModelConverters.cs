using AudibleDownloader.DAL.Models;
using AudibleDownloader.Models;

namespace AudibleDownloader.DAL;

public static class ModelConverters
{
    public static AudibleAuthor ToInternal(this Author dalModel)
    {
        return new AudibleAuthor()
        {
            Id = dalModel.Id,
            Asin = dalModel.Asin,
            Created = dalModel.Created,
            Link = dalModel.Link,
            Name = dalModel.Name
        };
    }

    public static AudibleBook ToInternal(this Book dalModel)
    {
        return new AudibleBook()
        {
            Id = dalModel.Id,
            Asin = dalModel.Asin,
            Isbn = dalModel.Isbn,
            Title = dalModel.Title,
            Length = dalModel.Length,
            Link = dalModel.Link,
            Released = dalModel.Released,
            Summary = dalModel.Summary,
            LastUpdated = dalModel.LastUpdated,
            Created = dalModel.Created,
            ShouldDownload = dalModel.ShouldDownload,
            IsTemp = dalModel.IsTemp,
        };
    }

    public static AudibleCategory ToInternal(this Category dalModel)
    {
        return new AudibleCategory()
        {
            Id = dalModel.Id,
            Name = dalModel.Name,
            Link = dalModel.Link,
            Created = dalModel.Created
        };
    }
    
    public static AudibleNarrator ToInternal(this Narrator dalModel)
    {
        return new AudibleNarrator()
        {
            Id = dalModel.Id,
            Name = dalModel.Name,
            Created = dalModel.Created
        };
    }

    public static AudibleSeries ToInternal(this Series dalModel)
    {
        return new AudibleSeries()
        {
            Id = dalModel.Id,
            Asin = dalModel.Asin,
            Name = dalModel.Name,
            Link = dalModel.Link,
            LastUpdated = dalModel.LastUpdated,
            LastChecked = dalModel.LastChecked,
            Created = dalModel.Created,
            ShouldDownload = dalModel.ShouldDownload,
        };
    }
    
    public static AudibleTag ToInternal(this Tag dalModel)
    {
        return new AudibleTag()
        {
            Id = dalModel.Id,
            Tag = dalModel.Name,
            Created = dalModel.Created
        };
    }
    
    public static AudiblePublisher ToInternal(this Publisher dalModel){
        return new AudiblePublisher()
        {
            Id = dalModel.Id,
            Name = dalModel.Name,
            Created = dalModel.Created
        };
    }
}