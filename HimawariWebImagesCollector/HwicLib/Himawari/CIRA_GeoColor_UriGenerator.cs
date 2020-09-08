namespace Hwic.Himawari
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using System.Threading;
    using System.Threading.Tasks;


    using HtmlAgilityPack;


    using Hwic.Loggings;
    using Hwic.Net;


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


        public IEnumerable<(DateTimeOffset, Uri)> GenerateFromDocument(
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

            var uriCollection = imgUrlStrings
                .Select(s => (this.GetImageTimeFromString(s), s))
                .Where(tup => tup.Item1 >= sinceTime)
                .Select(tup => (tup.Item1, new Uri(prefix + tup.Item2)));

            (var dt, var uri) = uriCollection.First();
            log.Here().Debug($"{dt}\n{uri}");

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


        public IEnumerable<string> FindImageUriPathStrings(
                string innerTextKeyword,
                HtmlDocument htmlDoc)
        {
            const string K_HREF_ATTR = "href";

            const string xpath = "//td/a";

            var log = this.GetLogger();

            var aTags = htmlDoc.DocumentNode.SelectNodes(xpath);
            if (aTags == null || aTags.Count == 0)
                throw new Exception($"No qualified nodes found with XPath: \"{xpath}\"");

            return aTags
                .Where(node => node.InnerText.Contains(innerTextKeyword, StringComparison.OrdinalIgnoreCase))
                .Select(node => node.Attributes[K_HREF_ATTR].Value);
        }


        /// <summary>
        /// Prepend time info for strings like "prefix_20200908030000.ext"
        /// </summary>
        /// <param name="urlStr"></param>
        /// <returns></returns>
        public DateTimeOffset GetImageTimeFromString(string urlStr)
        {
            //var log = this.GetLogger();

            const string sample = "YYYYMMddHHmmss";

            var lastUnderScore = urlStr.LastIndexOf("_");
            var lastDot = urlStr.LastIndexOf(".");

            if (lastUnderScore < 0 || lastDot < 0 || lastUnderScore > lastDot)
                throw new ArgumentException($"unexpected string: {urlStr}");

            var length = lastDot - lastUnderScore - 1;
            if (length != sample.Length)
                throw new ArgumentException($"illegal string: {urlStr}");

            var dateTimeStr = urlStr.Substring(lastUnderScore + 1, length);

            const int L_YEAR = 4;
            const int L_MONTH = 2;
            const int L_DAY = 2;
            const int L_HOUR = 2;
            const int L_MINUTE = 2;
            const int L_SECOND = 2;

            var offsetL = new[]
            {
                L_YEAR,
                L_MONTH,
                L_DAY,
                L_HOUR,
                L_MINUTE,
                L_SECOND
            };
            var field = new int[offsetL.Length];
            var offset = 0;
            for (var i = 0; i < field.Length; ++i)
            {
                var subStr = dateTimeStr.Substring(offset, offsetL[i]);

                //log.Debug($"#{i} {offset} {offsetL[i]}: {subStr}");

                field[i] = System.Convert.ToInt32(subStr);
                offset += offsetL[i];
            }
            var dt = new DateTimeOffset(
                year  : field[0],
                month : field[1],
                day   : field[2],
                hour  : field[3],
                minute: field[4],
                second: field[5],
                offset: TimeSpan.Zero  // UTC time
            );
            return dt;
        }
    }
}
