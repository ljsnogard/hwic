namespace Hwic.UnitTestings.Himawari
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Reflection;


    using FluentAssertions;


    using HtmlAgilityPack;


    using Hwic.Himawari;
    using Hwic.Net;


    using Xunit;
    using Xunit.Abstractions;


    public sealed partial class CIRA_Geocolor_UriGeneratorTest
    {
        private ITestOutputHelper Output { get; }


        public CIRA_Geocolor_UriGeneratorTest(ITestOutputHelper output)
            => this.Output = output;


        [Theory]
        [InlineData("192.168.7.11", 1080)]
        public void DownloadPage_ShouldFindUri(string socksProxyAddr, ushort socksAddrPort)
        {
            var proxyInfo = new Socks5ProxyInfo(socksProxyAddr, socksAddrPort);
            var generator = new CIRA_GeoColor_UriGenerator(proxyInfo);

            var uriCollection = generator.GenerateFromDocument(
                this.ReadTestHtmlDoc_(),
                DateTimeOffset.Now
            );
            uriCollection.Any().Should().BeTrue();
        }


        [Fact]
        public void ParseDownloadPage_ShouldFindUri()
        {
            var generator = new CIRA_GeoColor_UriGenerator();

            var htmlDoc = this.ReadTestHtmlDoc_();
            var uriStrings = generator.FindImageUriPathStrings(
                CIRA_GeoColor_UriGenerator.K_INNER_TEXT,
                htmlDoc
            );
            uriStrings.Any().Should().BeTrue();
        }


        private HtmlDocument ReadTestHtmlDoc_()
        {
            var filePath = @"Himawari\TestHtmlContent.html";
            var htmlDoc = new HtmlDocument();

            var path = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location),
                filePath
            );
            using var fileStream = new FileStream(path, FileMode.Open);
            htmlDoc.Load(fileStream);

            return htmlDoc;
        }
    }
}
