namespace Hwic.Himawari
{
    using HtmlAgilityPack;

    using Hwic.Loggings;
    using Hwic.Net;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;


    public class CIRA_GeoColor_UriGenerator
    {
        public const string PageUriStr =
            "http://rammb.cira.colostate.edu/ramsdis/online/archive_hi_res.asp?data_folder=himawari-8/full_disk_ahi_true_color";


        public const string K_INNER_TEXT = "Hi-Res Image";


        private readonly Socks5ProxyInfo socks5Proxy_;


        private readonly Uri indexPageUri_;


        public CIRA_GeoColor_UriGenerator(Socks5ProxyInfo proxy)
        {
            this.socks5Proxy_ = proxy;
            this.indexPageUri_ = new Uri(PageUriStr);
        }


        public CIRA_GeoColor_UriGenerator() : this(null)
        { }


        public IEnumerable<Uri> GenerateFromDocument(
                HtmlDocument htmlDoc,
                DateTimeOffset sinceTime)
        {
            var log = this.GetLogger();
            var imgUrlStrings = this.FindImageUriPathStrings(K_INNER_TEXT, htmlDoc);

            var scheme = this.indexPageUri_.Scheme;
            var host = this.indexPageUri_.Host;
            var path = this.indexPageUri_.AbsolutePath;

            path = path.Substring(0, path.LastIndexOf("/"));
            var prefix = $"{scheme}://{host}{path}/";

            log.Here().Debug(prefix);

            var uriCollection = imgUrlStrings.Select(s => new Uri(prefix + s));

            log.Here().Debug(uriCollection.First().ToString());

            return uriCollection;
        }


        private async Task<ulong> GetPageHtmlContentAsync_(Func<Memory<byte>, CancellationToken, Task> onRecvData)
        {
            var dlconfig =
                this.socks5Proxy_ is null ?
                    new HttpDownloadConfig() :
                    new HttpDownloadConfig(this.socks5Proxy_);

            var dlworker = new HttpClientDownloadWorker(
                dlconfig,
                this.indexPageUri_
            );

            return await dlworker.StartAsync(
                onRecvData,
                tk => Task.FromResult(true)
            );
        }


        public async Task<HtmlDocument> DownloadIndexPageAsync()
        {
            var log = this.GetLogger();

            using var memoryStream = new MemoryStream();

            var contLength = await GetPageHtmlContentAsync_(
                async (data, token) => await memoryStream.WriteAsync(data)
            );

            log.Here().Information($"Download page completed: {contLength} bytes downloaded.");

            var htmlDoc = new HtmlDocument();

            memoryStream.Position = 0L;
            htmlDoc.Load(memoryStream);

            return htmlDoc;
        }


        public List<string> FindImageUriPathStrings(
                string innerTextKeyword,
                HtmlDocument htmlDoc)
        {
            const string K_HREF_ATTR = "href";

            const string xpath = "//td/a";

            var log = this.GetLogger();

            var aTags = htmlDoc.DocumentNode.SelectNodes(xpath);
            if (aTags == null || aTags.Count == 0)
                throw new Exception($"No qualified nodes found with XPath: \"{xpath}\"");

            var imgUrlStrings = aTags
                .Where(node => node.InnerText.Contains(innerTextKeyword, StringComparison.OrdinalIgnoreCase))
                .Select(node => node.Attributes[K_HREF_ATTR].Value)
                .ToList();

            //var strBuilder = new StringBuilder();
            //foreach (var s in imgUrlStrings)
            //    strBuilder.AppendLine(s);

            //log.Here().Debug(strBuilder.ToString());

            return imgUrlStrings;
        }
    }
}
