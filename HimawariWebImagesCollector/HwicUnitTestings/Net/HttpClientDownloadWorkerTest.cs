namespace Hwic.UnitTestings.Net
{
    using System;

    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using FluentAssertions;


    using Hwic.Net;
    using Hwic.Storings;


    using Nito.AsyncEx;


    using Serilog;


    using Xunit;
    using Xunit.Abstractions;


    public class HttpClientDownloadWorkerTest
    {
        private ITestOutputHelper Output { get; }


        public HttpClientDownloadWorkerTest(ITestOutputHelper output)
            => this.Output = output;


        [Theory]
        [InlineData(@"https://www.bilibili.com/blackboard/aboutUs.html")]
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
            var dlworker = new HttpClientDownloadWorker(dlconfig, uri);
            var stworker = new LocalFileStorageWorker(stconfig, uri);

            var dataQueue = new AsyncProducerConsumerQueue<Memory<byte>>();

            var dlTask = dlworker.StartAsync(
                dataQueue.EnqueueAsync,
                tk => Task.FromResult(true)
            );
            var stTask = stworker.StoreAsync(
                dataQueue.DequeueAsync,
                dataQueue.OutputAvailableAsync
            );

            var dlSize = await dlTask;
            dataQueue.CompleteAdding();

            var stSize = await stTask;
            (dlSize == stSize).Should().BeTrue();
        }
    }
}
