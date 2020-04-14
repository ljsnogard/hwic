namespace Hwic.UnitTestings
{
    using System;

    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using FluentAssertions;


    using Hwic.Net;
    using Hwic.Pipes;


    using Serilog;


    using Xunit;
    using Xunit.Abstractions;


    public class HttpClientDownloadWorkerTest
    {
        private ITestOutputHelper Output { get; }


        public HttpClientDownloadWorkerTest(ITestOutputHelper output)
            => this.Output = output;


        [Theory]
        [InlineData(@"https://bastille.readthedocs.io/en/latest/chapters/usage.html")]
        [InlineData(@"https://www.pconline.com.cn/index.html")]
        public async Task TestDownloadAsync(string uriStr)
        {
            var config = new HttpDownloadConfig();
            var uri = new Uri(uriStr);
            var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            var filename = Path.GetFileName(uri.AbsolutePath);

            File.Exists(filename).Should().BeFalse();

            var worker = new HttpClientDownloadWorker(config, uri, log);
            
            var filePipe = StreamDataPipe.Create(() => new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write));

            var t = worker.StartAsync(filePipe);
            await Task.WhenAny(t, Task.Delay(TimeSpan.FromSeconds(30)));
            t.IsCompleted.Should().BeTrue();

            var dlSize = t.Result;
            dlSize.Should().BeGreaterThan(0u);

            if (filePipe is IDataPipeProducerEnd producerPipe)
                await producerPipe.CloseAsync();

            File.Exists(filename).Should().BeTrue();
        }
    }
}
