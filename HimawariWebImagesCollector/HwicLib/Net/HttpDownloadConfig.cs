namespace Hwic.Net
{
    using System;
    using System.Collections.Generic;

    using System.Linq;
    using System.Net.Http;


    using Hwic.Abstractings;


    public class HttpDownloadConfig : IDownloadConfig
    {
        public const uint DEFAULT_BUFFER_SIZE = 4096u;


        public IEnumerable<Socks5ProxyInfo> Proxies { get; }


        public readonly uint BufferSize;


        public HttpDownloadConfig() :
            this(DEFAULT_BUFFER_SIZE, Enumerable.Empty<Socks5ProxyInfo>())
        { }


        public HttpDownloadConfig(params Socks5ProxyInfo[] infoItems) :
            this(DEFAULT_BUFFER_SIZE, infoItems)
        { }


        public HttpDownloadConfig(in uint bufferSize, IEnumerable<Socks5ProxyInfo> proxies)
        {
            if (bufferSize == 0u)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            this.Proxies = proxies;
            this.BufferSize = bufferSize;
        }
    }



    public static class HttpDownloadConfigHttpClientExtensions
    {
        private sealed class ProxyInfoEqComparer : IEqualityComparer<Socks5ProxyInfo>
        {
            bool IEqualityComparer<Socks5ProxyInfo>.Equals(Socks5ProxyInfo x, Socks5ProxyInfo y)
            {
                return
                    x.Host == y.Host &&
                    x.Port == y.Port &&
                    x.User == y.User &&
                    x.Credential == y.Credential;
            }


            int IEqualityComparer<Socks5ProxyInfo>.GetHashCode(Socks5ProxyInfo obj)
            {
                unchecked
                {
                    return 
                        (obj.Host.GetHashCode() * 3) ^
                        (obj.Port * 5) ^
                        (obj.User.GetHashCode() * 7) ^
                        (obj.Credential.GetHashCode() * 11);
                }
            }
        }



        private sealed class HttpClientManager
        {
            private readonly object mutex_;

            private readonly ProxyInfoEqComparer eq_;

            private readonly Dictionary<Socks5ProxyInfo, HttpClient> clientDict_;

            private HttpClient defaultClient_;

            public HttpClientManager()
            {
                this.mutex_ = new object();
                this.eq_ = new ProxyInfoEqComparer();
                this.clientDict_ = new Dictionary<Socks5ProxyInfo, HttpClient>(this.eq_);
            }

            public HttpClient DefaultClient
            {
                get
                {
                    lock (this.mutex_)
                    {
                        if (this.defaultClient_ is null)
                            this.defaultClient_ = new HttpClient();
                    }
                    return this.defaultClient_;
                }
            }


            public HttpClient GetClient(Socks5ProxyInfo proxyInfo)
            {
                lock (this.mutex_)
                {
                    if (false == this.clientDict_.TryGetValue(proxyInfo, out var client))
                    {
                        var proxy = proxyInfo.CreateWebProxy();
                        var handler = new HttpClientHandler { Proxy = proxy };
                        client = new HttpClient(
                            handler: handler,
                            disposeHandler: true
                        );
                        this.clientDict_[proxyInfo] = client;
                    }
                    return client;
                }
            }
        }



        private static readonly HttpClientManager mgr_ = new HttpClientManager();



        public static HttpClient GetHttpClient(this HttpDownloadConfig config)
        {
            if (false == config.Proxies.Any())
                return mgr_.DefaultClient;

            var proxyInfo = config.Proxies.First();
            return mgr_.GetClient(proxyInfo);
        }
    }
}
