namespace Hwic.Net
{
    using System.Collections.Generic;

    using System.Net.Http;


    using Hwic.Abstractings;


    public class HttpDownloadConfig : IDownloadConfig
    {
        public IEnumerable<Socks5ProxyInfo> Proxies { get; }
    }



    public static class HttpDownloadConfigHttpClientExtensions
    {
        public static HttpClient GetHttpClient(this HttpDownloadConfig config)
        {
            throw new System.NotImplementedException();
        }
    }
}
