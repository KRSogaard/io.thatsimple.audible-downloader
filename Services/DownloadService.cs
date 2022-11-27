﻿using System.Net;
using System.Net.Sockets;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Services;
using NLog;

namespace AudibleDownloader.Services;

public class ProxyWrapper
{
    public string Host { get; set; }
    public int Port { get; set; }
}

public class DownloadService
{
    private static readonly TimeSpan RequestTimeOut = new(0, 0, 5);
    private static readonly int RefreshProxyTime = 1000 * 60 * 60;
    private static int BadProxyMinutes = 10;
    private static readonly int BadReportLimit = 3;
    private Dictionary<ProxyWrapper, List<DateTime>> badProxyCounter;
    private List<Tuple<HttpClient, ProxyWrapper>> clients;
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    private List<ProxyWrapper> proxies;

    private readonly object proxyLock = new();
    private readonly Random random;
    private readonly StorageService storageService;

    private readonly List<string> userAgents = new()
    {
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",
        "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1",
        "Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36"
    };

    public DownloadService(StorageService storageService)
    {
        random = new Random();
        this.storageService = storageService;
        badProxyCounter = new Dictionary<ProxyWrapper, List<DateTime>>();

        CreatingHttpClients().Wait();

        log.Info("Creating timer to refresh proxies every " + TimeSpan.FromMilliseconds(RefreshProxyTime).TotalHours +
                 " hours");
        var timer = new Timer(state =>
        {
            log.Info("Refreshing proxies");
            CreatingHttpClients().Wait();
        }, null, RefreshProxyTime, RefreshProxyTime);
    }

    private async Task CreatingHttpClients()
    {
        var proxyUrl = Config.Get("PROXY_LIST");
        log.Info("Generating new proxy list from: " + proxyUrl);
        var tempClient = new HttpClient();
        var response = await tempClient.GetAsync(proxyUrl);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            log.Error("Error getting proxy list: " + response.StatusCode);
            throw new RetryableException("Failed to get proxy list");
        }

        var newProxies = new List<ProxyWrapper>();
        foreach (var line in (await response.Content.ReadAsStringAsync()).Trim().Split("\n"))
        {
            var parts = line.Trim().Split(":");
            if (parts.Length < 2)
            {
                log.Warn("Proxy line is invalid: " + line);
                continue;
            }

            var proxy = new ProxyWrapper();
            proxy.Host = parts[0].Trim();
            proxy.Port = int.Parse(parts[1].Trim());
            newProxies.Add(proxy);
        }

        var newClients = new List<Tuple<HttpClient, ProxyWrapper>>();
        foreach (var proxy in newProxies)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = false
            };
            handler.Proxy = new WebProxy(new Uri("socks5://" + proxy.Host + ":" + proxy.Port));
            handler.UseProxy = true;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            newClients.Add(new Tuple<HttpClient, ProxyWrapper>(client, proxy));
        }

        lock (proxyLock)
        {
            proxies = newProxies;
            clients = newClients;
            badProxyCounter = new Dictionary<ProxyWrapper, List<DateTime>>();
        }
    }

    private Tuple<HttpClient, ProxyWrapper> GetClient()
    {
        CleanBadProxyCounter();

        if (clients.Count == 0) throw new RetryableException("No proxies available");

        var index = random.Next(0, clients.Count - 1);

        while (IsProxyBad(index))
        {
            log.Trace("Proxy has been reported as bad, trying another one [" + clients[index].Item2.Host + ":" +
                      clients[index].Item2.Port + "]");
            index = random.Next(0, clients.Count - 1);
        }

        log.Debug("Using client index: " + index + " for proxy: " + clients[index].Item2.Host + ":" +
                  clients[index].Item2.Port);
        return clients[index];
    }

    private bool IsProxyBad(int index)
    {
        lock (proxyLock)
        {
            var client = clients[index];
            if (!badProxyCounter.ContainsKey(client.Item2)) return false;

            return badProxyCounter[client.Item2].Count() > BadReportLimit;
        }
    }

    private void ReportBadProxy(ProxyWrapper proxy)
    {
        log.Warn("Proxy reported as bad: " + proxy.Host + ":" + proxy.Port);
        lock (proxyLock)
        {
            if (!badProxyCounter.ContainsKey(proxy)) badProxyCounter[proxy] = new List<DateTime>();
            badProxyCounter[proxy].Add(DateTime.Now);
        }

        CleanBadProxyCounter();
    }

    private void CleanBadProxyCounter()
    {
        lock (proxyLock)
        {
            foreach (var proxy in badProxyCounter.Keys.ToList())
            {
                badProxyCounter[proxy] =
                    badProxyCounter[proxy].Where(x => x > DateTime.Now.AddMinutes(-30)).ToList();
                if (badProxyCounter[proxy].Count == 0) badProxyCounter.Remove(proxy);
            }
        }
    }

    private string GetUserAgent()
    {
        var index = random.Next(0, userAgents.Count - 1);
        return userAgents[index];
    }

    public async Task<DownloadResponse> DownloadHtml(string url)
    {
        log.Info("Request to download html from url: " + url);
        var cached = await storageService.GetHtmlCache(url);
        if (cached != null)
        {
            log.Trace("Using cached html for url: " + url);
            return new DownloadResponse
            {
                StatusCode = HttpStatusCode.OK,
                Data = cached
            };
        }

        var client = GetClient();
        try
        {
            var s_cts = new CancellationTokenSource();
            s_cts.CancelAfter(RequestTimeOut);
            var responseMessage = await client.Item1.GetAsync(url, s_cts.Token);

            if (responseMessage.StatusCode == HttpStatusCode.Moved ||
                responseMessage.StatusCode == HttpStatusCode.MovedPermanently ||
                responseMessage.StatusCode == HttpStatusCode.Redirect)
            {
                log.Warn("Got redirect, must be a bad proxy sending to retry. From [" + url + "] to [" +
                         responseMessage.Headers.Location + "] with status code: " + responseMessage.StatusCode);
                ReportBadProxy(client.Item2);
                throw new RetryableException("Got redirect");
            }

            var data = await responseMessage.Content.ReadAsStringAsync();

            if (!data.Contains(@"<html lang=""en"">"))
            {
                log.Error("Did not get english html for url:  (" + url +
                          ") with proxy [" + client.Item2.Host + ":" + client.Item2.Port + "]");
                ReportBadProxy(client.Item2);
                throw new RetryableException("Invalid html response");
            }

            if (IsRedirect(data))
            {
                log.Warn("Got front page, maybe the proxy did not work?  (" + url +
                         ") with proxy [" + client.Item2.Host + ":" + client.Item2.Port + "]");
                ReportBadProxy(client.Item2);
                throw new RetryableException("Got front page");
            }

            storageService.SetHtmlCache(url, data);
            return new DownloadResponse
            {
                StatusCode = responseMessage.StatusCode,
                Data = data
            };
        }
        catch (TaskCanceledException e)
        {
            log.Warn("HTTP request timed out for url: " + url);
            ReportBadProxy(client.Item2);
            throw new RetryableException();
        }
        catch (HttpRequestException e)
        {
            if (e.InnerException is SocketException)
            {
                log.Warn("Socket exception for url (" + url + ") with proxy [" + client.Item2.Host + ":" +
                         client.Item2.Port + "]");
                ReportBadProxy(client.Item2);
                throw new RetryableException("Socket exception, bad proxy?");
            }

            throw e;
        }
    }

    public async Task DownloadImage(string imageUrl, string asin)
    {
        var client = GetClient();
        try
        {
            log.Debug("Downloading image for asin: " + asin);
            var s_cts = new CancellationTokenSource();
            s_cts.CancelAfter(RequestTimeOut);
            var fileBytes = await client.Item1.GetByteArrayAsync(new Uri(imageUrl), s_cts.Token);
            await storageService.SaveImage(asin, fileBytes);
        }
        catch (TaskCanceledException e)
        {
            log.Warn("HTTP request timed out for url: " + imageUrl);
            ReportBadProxy(client.Item2);
            throw new RetryableException();
        }
        catch (HttpRequestException e)
        {
            if (e.InnerException is SocketException)
            {
                log.Warn("Socket exception for url (" + imageUrl + ") with proxy [" + client.Item2.Host + ":" +
                         client.Item2.Port + "]");
                ReportBadProxy(client.Item2);
                throw new RetryableException("Socket exception, bad proxy?");
            }

            throw e;
        }
    }

    private bool IsRedirect(string html)
    {
        if (html.Contains("<title>Livres audio et plus | Votre premier livre est gratuit! | Audible.ca</title>"))
            return true;

        return false;
    }
}

public class DownloadResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string Data { get; set; }
    public bool Cached { get; set; }
}