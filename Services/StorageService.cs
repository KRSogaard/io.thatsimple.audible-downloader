using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;

namespace AudibleDownloader.Services
{
    public class StorageService
    {
        
        private MinioClient minioClient;
        private static string cacheBucketName = "audible-webcache";
        private static string imagesBucketName = "audiobook-images";
        
        public StorageService()
        {
            minioClient = new MinioClient()
                .WithEndpoint(Config.Get("MINIO_END_POINT") + ":" + int.Parse(Config.Get("MINIO_PORT")))
                .WithCredentials(Config.Get("MINIO_ACCESS_KEY"), Config.Get("MINIO_SECRET_KEY"))
                // .WithSSL(secure)
                .Build();
        }
        
        
        public async Task<string?> GetHtmlCache(string url)
        {
            string fileName = UrlToHash(url);

            string html = null;
            try
            {
                await minioClient.GetObjectAsync(cacheBucketName, fileName, (stream) =>
                {
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        html = reader.ReadToEnd();
                    }
                });
            } catch (ObjectNotFoundException e)
            {
                return null;
            }

            return html;
        }
        
        public async Task SetHtmlCache(string url, string html)
        {
            string fileName = UrlToHash(url);
            await minioClient.PutObjectAsync(cacheBucketName, fileName, 
                new MemoryStream(Encoding.UTF8.GetBytes(html)),
                html.Length, "text/html");
        }
        
        public async Task<bool> HasImage(string asin)
        {
            try
            {
                ObjectStat objectStat = await minioClient.StatObjectAsync(imagesBucketName, ImageKey(asin));
                return true;
            }
            catch (ObjectNotFoundException e)
            {
                return false;
            }
        }
        
        private string UrlToHash(string url) {
            if (url.Contains("audible.com/pd/")) {
                // USE ASIN as the hash
                return "books-" + url.Split('?')[0].Split('/').Last() + ".html";
            }
            if (url.Contains("series/"))
            {
                return "series-" + url.Split('?')[0].Split('/').Last() + ".html";
            }
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(url);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

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
}
