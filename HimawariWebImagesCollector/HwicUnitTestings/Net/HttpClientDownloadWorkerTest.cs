namespace Hwic.UnitTestings.Net
{
    using System;

    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using FluentAssertions;


    using Hwic.Net;
    using Hwic.Pipes;
    using Hwic.Storings;


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
            var log = new LoggerConfiguration().WriteTo.File("unit-test.log").CreateLogger();
            var filename = Path.GetFileName(uri.AbsolutePath);

            File.Exists(filename).Should().BeFalse();

            this.Output.WriteLine($"Start download from {uri} and save to file {filename}");

            var worker = new HttpClientDownloadWorker(config, uri, log);
            
            var filePipe = StreamDataPipe.Create(() => new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write));

            var t = worker.StartAsync(filePipe);
            await Task.WhenAny(t, Task.Delay(TimeSpan.FromSeconds(30)));
            t.IsCompleted.Should().BeTrue();

            var dlSize = t.Result;
            dlSize.Should().BeGreaterThan(0u);

            this.Output.WriteLine($"Wrote {dlSize} bytes data into file {filename}.");

            if (filePipe is IDataPipeProducerEnd producerPipe)
                await producerPipe.CloseAsync();

            File.Exists(filename).Should().BeTrue();
        }


        [Theory]
        [InlineData(@"https://bastille.readthedocs.io/en/latest/chapters/usage.html")]
        [InlineData(@"https://www.pconline.com.cn/index.html")]
        public async Task TestDownloadAndStoreAsync(string uriStr)
        {
            var dlconfig = new HttpDownloadConfig();

            var tempdir = Path.GetTempPath();
            var stconfig = new LocalFileStorageConfig(tempdir);

            var logger = new LoggerConfiguration()
                .WriteTo.File($"{nameof(TestDownloadAndStoreAsync)}.log")
                .CreateLogger();

            logger.Information("temp dir: {currdir}", tempdir);

            var uri = new Uri(uriStr);
            var dlworker = new HttpClientDownloadWorker(dlconfig, uri, logger);
            var stworker = new LocalFileStorageWorker(stconfig, uri, logger);
            var pipe = StreamDataPipe.Create<MemoryStream>();

            var dlTask = dlworker.StartAsync(pipe);
            var stTask = stworker.StoreAsync(pipe);

            await Task.WhenAll(dlTask, stTask);

            if (pipe is IDataPipeProducerEnd p)
                await p.CloseAsync();

            if (pipe is IDataPipeConsumerEnd c)
                await c.CloseAsync();
        }
    }
}
