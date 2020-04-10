namespace Hwic.UnitTestings
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;


    using MihaZupan;


    using Xunit;
    using Xunit.Abstractions;


    public class Socks5HttpDownloadTest
    {
        public ITestOutputHelper Output { get; }


        public Socks5HttpDownloadTest(ITestOutputHelper output)
            => this.Output = output;


        [Theory]
        [InlineData(
            @"https://himawari8-dl.nict.go.jp/himawari.asia/img/D531106/20d/550/2020/03/09/044000_8_2.png",
            "127.0.0.1",
            1080,
            30u,
            "2020_0309_044000_8_2.png"
        )]
        public async Task Test1(
                string httpUri,
                string socksProxyAddr,
                int socksAddrPort,
                uint timeoutSec,
                string filename)
        {
            var proxy = new HttpToSocks5Proxy(socksProxyAddr, socksAddrPort);
            var handler = new HttpClientHandler { Proxy = proxy };

            using var httpClient = new HttpClient(
                handler: handler,
                disposeHandler: true
            );

            using var request = new HttpRequestMessage(HttpMethod.Get, httpUri);
            var sendRequestTask = httpClient.SendAsync(request);

            this.Output.WriteLine($"Sending request through proxy {socksProxyAddr}:{socksAddrPort}, target url: {httpUri} ");

            await Task.WhenAny(
                Task.Delay(TimeSpan.FromSeconds(timeoutSec)),
                sendRequestTask
            );
            if (false == sendRequestTask.IsCompleted)
            {
                this.Output.WriteLine($"No resp within timeout {timeoutSec} secs.");
                Assert.True(sendRequestTask.IsCompleted);
            }

            using var httpContStream = await (await sendRequestTask).Content.ReadAsStreamAsync();
            using var fileContStream = new FileStream(
                path: filename,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.None
            );
            await httpContStream.CopyToAsync(fileContStream);
        }
    }
}
