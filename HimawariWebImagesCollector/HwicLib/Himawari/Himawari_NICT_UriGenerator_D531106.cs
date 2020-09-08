namespace Hwic.Himawari
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Flurl;


    public class Himawari_NICT_UriGenerator_D531106
    {
        public int ResolutionOption => 20;


        public string Host => "himawari8-dl.nict.go.jp";


        public string Prefix => $@"himawari.asia/img/D531106/{this.ResolutionOption}d/550";


        public string ImageFormat => "png";


        public IEnumerable<Uri> Generate(DateTimeOffset imageTime)
        {
            const string NUM_FMT = "d2";

            var utcTime = imageTime.UtcDateTime;
            var year = utcTime.Year.ToString();
            var month = utcTime.Month.ToString(NUM_FMT);
            var day = utcTime.Day.ToString(NUM_FMT);
            var hour = utcTime.Hour.ToString(NUM_FMT);
            var minute = GetTenCeiling_(utcTime.Minute).ToString(NUM_FMT);
            const string second = "00";

            var moment = $"{hour}{minute}{second}";

            var urlStr = Url.Combine(
                this.Host,
                this.Prefix,
                year, month, day
            );

            var fileNames =
                from x in Enumerable.Range(0, this.ResolutionOption)
                from y in Enumerable.Range(0, this.ResolutionOption)
                select $"{moment}_{x}_{y}.{this.ImageFormat}";

            foreach(var fn in fileNames)
            {
                var uriStr = Url.Combine(urlStr, fn);
                yield return new Uri($"https://{uriStr}");
            }

            static int GetTenCeiling_(int v)
                => v - (v % 10);
        }
    }
}
