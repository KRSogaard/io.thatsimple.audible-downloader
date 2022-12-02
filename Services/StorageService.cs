using System.Security.Cryptography;
using System.Text;
using AudibleDownloader.Utils;
using Minio;
using Minio.Exceptions;
using NLog;

namespace AudibleDownloader.Services;

public class StorageService
{
    private static readonly string cacheBucketName = "audible-webcache";
    private static readonly string imagesBucketName = "audiobook-images";
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    private readonly MinioClient minioClient;

    public StorageService()
    {
        minioClient = new MinioClient()
            .WithEndpoint(Config.Get("MINIO_END_POINT") + ":" + int.Parse(Config.Get("MINIO_PORT")))
            .WithCredentials(Config.Get("MINIO_ACCESS_KEY"), Config.Get("MINIO_SECRET_KEY"))
            // .WithSSL(secure)
            .Build();

        log.Info("Creaing MinIO client to \"" + Config.Get("MINIO_END_POINT") + ":" +
                 int.Parse(Config.Get("MINIO_PORT")) + "\"");
    }


    public async Task<string?> GetStringCache(string url)
    {
        var fileName = UrlToHash(url);

        string html = null;
        try
        {
            await minioClient.GetObjectAsync(cacheBucketName, fileName, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            });
        }
        catch (ObjectNotFoundException e)
        {
            return null;
        }

        return html;
    }
    
    public async Task SetStringCache(string url, string content, string contentType = "text/html")
    {
        var fileName = UrlToHash(url);
        var stream = content.ToStream();
        await minioClient.PutObjectAsync(cacheBucketName, fileName,
            stream,
            stream.Length, contentType);
    }

    public async Task<bool> HasImage(string asin)
    {
        try
        {
            var objectStat = await minioClient.StatObjectAsync(imagesBucketName, ImageKey(asin));
            return true;
        }
        catch (ObjectNotFoundException e)
        {
            return false;
        }
    }

    private string UrlToHash(string url)
    {
        if (url.Contains("audible.com/pd/"))
            // USE ASIN as the hash
            return "books-" + url.Split('?')[0].Split('/').Last() + ".html";
        if (url.Contains("series/")) 
            return "series-" + url.Split('?')[0].Split('/').Last() + ".html";
        if (url.Contains("https://api.audible.com"))
        {
            using (var md5 = MD5.Create())
            {
                string groups = url.Split("response_groups=").Last();
                var inputBytes = Encoding.ASCII.GetBytes(groups);
                var hashBytes = Convert.ToHexString(md5.ComputeHash(inputBytes));
                return "api-" + url.Split('?')[0].Split('/').Last() + "-" + hashBytes + ".json";
            }
        }
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(url);
            var hashBytes = md5.ComputeHash(inputBytes);

            return "other-" + Convert.ToHexString(hashBytes) + ".html";
        }
    }

    private string ImageKey(string asin)
    {
        return asin + ".jpg";
    }

    public async Task SaveImage(string asin, byte[] fileBytes)
    {
        var length = fileBytes.Length;
        await minioClient.PutObjectAsync(imagesBucketName, ImageKey(asin),
            new MemoryStream(fileBytes), length, "image/jpeg");
    }
}