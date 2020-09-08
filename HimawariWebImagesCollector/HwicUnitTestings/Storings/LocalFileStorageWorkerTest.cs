namespace Hwic.UnitTestings.Storings
{
    using System;

    using System.IO;

    using System.Text;

    using System.Threading;
    using System.Threading.Tasks;


    using FluentAssertions;


    using Hwic.Net;
    using Hwic.Storings;


    using Nito.AsyncEx;


    using Serilog;


    using Xunit;
    using Xunit.Abstractions;


    public class LocalFileStorageWorkerTest
    {
        [Fact]
        public async Task TestStoreFileAsync()
        {
            var tempdir = Path.GetTempPath();
            var stconfig = new LocalFileStorageConfig(tempdir);

            var name = @"https://localhost/TestStoreFileAsync/sample.txt";
            var uri = new Uri(name);

            var logger = new LoggerConfiguration()
                .WriteTo.File($"{nameof(TestStoreFileAsync)}.log")
                .CreateLogger();

            var stworker = new LocalFileStorageWorker(stconfig);

            var queue = new AsyncProducerConsumerQueue<Memory<byte>>();

            var tks = new CancellationTokenSource();

            var stTask = stworker.StoreAsync(
                uri,
                queue.DequeueAsync,
                queue.OutputAvailableAsync,
                tks.Token
            );

            queue.Enqueue(Encoding.UTF8.GetBytes(name));
            queue.CompleteAdding();

            await Task.WhenAny(
                stTask,
                Task.Delay(TimeSpan.FromSeconds(30))
            );
            if (stTask.IsCompleted == false)
                tks.Cancel();
        }
    }
}
