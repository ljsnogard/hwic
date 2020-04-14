namespace Hwic.Net
{
    using MihaZupan;


    public sealed class Socks5ProxyInfo
    {
        public string Host { get; }


        public ushort Port { get; }


        public string User { get; }


        public string Credential { get; }


        public Socks5ProxyInfo(
                string host,
                ushort port,
                string user,
                string credential)
        {
            this.Host = host;
            this.Port = port;
            this.User = user;
            this.Credential = credential;
        }


        public Socks5ProxyInfo(
                string host,
                ushort port)
        {
            this.Host = host;
            this.Port = port;
            this.User = string.Empty;
            this.Credential = string.Empty;
        }


        public HttpToSocks5Proxy CreateWebProxy()
        {
            if (string.IsNullOrEmpty(this.User))
            {
                return new HttpToSocks5Proxy(
                    this.Host,
                    this.Port
                );
            }
            else
            {
                return new HttpToSocks5Proxy(
                    this.Host,
                    this.Port,
                    this.User,
                    this.Credential
                );
            }
        }
    }
}
