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
            Name = dalModel.Name
        };
    }
}