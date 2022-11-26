using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Services;
using Microsoft.Extensions.Hosting.Internal;
using NLog;

namespace AudibleDownloader
{
    public class ProxyWrapper
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
    
    public class DownloadService
    {
        private StorageService storageService;
        private Logger log = LogManager.GetCurrentClassLogger();
        private List<Tuple<HttpClient, ProxyWrapper>> clients;
        private List<ProxyWrapper> proxies;
        private Random random;

        private List<String> userAgents = new List<string>()
        {
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1",
            "Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36",
        };
        
        public DownloadService(StorageService storageService)
        {
            random = new Random();
            this.storageService = storageService;
            CreatingHttpClient().Wait();
        }

        private async Task CreatingHttpClient()
        {
            string proxyUrl = Config.Get("PROXY_LIST");
            log.Info("Creating new DownloadService with proxy list: " + proxyUrl);
            HttpClient tempClient = new HttpClient();
            HttpResponseMessage response = await tempClient.GetAsync(proxyUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                log.Error("Error getting proxy list: " + response.StatusCode);
                throw new RetryableException("Failed to get proxy list");
            }

            proxies = new List<ProxyWrapper>();
            foreach (string line in (await response.Content.ReadAsStringAsync()).Trim().Split("\n"))
            {
                string[] parts = line.Trim().Split(":");
                if (parts.Length < 2)
                {
                    log.Warn("Proxy line is invalid: " + line);
                    continue;
                }

                ProxyWrapper proxy = new ProxyWrapper();
                proxy.Host = parts[0].Trim();
                proxy.Port = int.Parse(parts[1].Trim());
                proxies.Add(proxy);
            }
            
            clients = new List<Tuple<HttpClient, ProxyWrapper>>();
            foreach (ProxyWrapper proxy in proxies)
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    AllowAutoRedirect = false
                };
                handler.Proxy = new WebProxy(new Uri("socks5://"+proxy.Host+":" + proxy.Port));
                handler.UseProxy = true;
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                clients.Add(new Tuple<HttpClient, ProxyWrapper>(client, proxy));
            }
        }

        private Tuple<HttpClient, ProxyWrapper> GetClient()
        {
            int index = random.Next(0, clients.Count - 1);
            log.Debug("Using client index: " + index + " for proxy: " + clients[index].Item2.Host + ":" + clients[index].Item2.Port);
            return clients[index];
        }

        private string GetUserAgent()
        {
            int index = random.Next(0, userAgents.Count - 1);
            return userAgents[index];
        }
        
        public async Task<DownloadResponse> DownloadHtml(string url)
        {
            log.Info("Request to download html from url: " + url);
            string? cached = await storageService.GetHtmlCache(url);
            if (cached != null)
            {
                log.Trace("Using cached html for url: " + url);
                return new DownloadResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = cached
                };
            }

            try
            {
                Tuple<HttpClient, ProxyWrapper> client = GetClient();
                CancellationTokenSource s_cts = new CancellationTokenSource();
                s_cts.CancelAfter(new TimeSpan(0, 0, 30));
                HttpResponseMessage responseMessage = await client.Item1.GetAsync(url, s_cts.Token);

                if (responseMessage.StatusCode == HttpStatusCode.Moved ||
                    responseMessage.StatusCode == HttpStatusCode.MovedPermanently ||
                    responseMessage.StatusCode == HttpStatusCode.Redirect)
                {
                    log.Warn("Got redirect, must be a bad proxy sending to retry. From [" + url + "] to [" + responseMessage.Headers.Location + "] with status code: " + responseMessage.StatusCode);
                    throw new RetryableException("Got redirect");
                }

                string data = await responseMessage.Content.ReadAsStringAsync();

                if (!data.Contains(@"<html lang=""en"">"))
                {
                    log.Error("Did not get english html for url: " + url);
                    throw new RetryableException("Invalid html response");
                }

                if (IsRedirect(data))
                {
                    log.Warn("Got front page, maybe the proxy did not work?");
                    throw new RetryableException("Got front page");
                }

                storageService.SetHtmlCache(url, data);
                return new DownloadResponse()
                {
                    StatusCode = responseMessage.StatusCode,
                    Data = data
                };
            }
            catch (TaskCanceledException e)
            {
                log.Warn("HTTP request timed out for url: " + url);
                throw new RetryableException();
            }
            catch (RetryableException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public async Task DownloadImage(string imageUrl, string asin)
        {
            log.Debug("Downloading image for asin: " + asin);
            Tuple<HttpClient, ProxyWrapper> client = GetClient();
            CancellationTokenSource s_cts = new CancellationTokenSource();
            s_cts.CancelAfter(new TimeSpan(0, 0, 30));
            var fileBytes = await client.Item1.GetByteArrayAsync(new Uri(imageUrl), s_cts.Token);
            await storageService.SaveImage(asin, fileBytes);
        }

        private bool IsRedirect(string html)
        {
            if (html.Contains("<title>Livres audio et plus | Votre premier livre est gratuit! | Audible.ca</title>"))
            {
                return true;
            }

            return false;
        }
    }

    public class DownloadResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Data { get; set; }
        public bool Cached { get; set; }
    }
}
