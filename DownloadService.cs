using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleDownloader
{
    public static class DownloadService
    {
        public static Task<DownloadResponse> DownloadHtml(string url)
        {
            throw new NotImplementedException();
        }

        internal static Task DownloadImage(string imageUrl, string asin)
        {
            throw new NotImplementedException();
        }
    }

    public class DownloadResponse
    {
        public int StatusCode { get; set; }
        public string Data { get; set; }
        public bool Cached { get; set; }
    }
}
