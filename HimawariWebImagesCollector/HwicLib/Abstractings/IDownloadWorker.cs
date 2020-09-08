namespace Hwic.Abstractings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    public interface IDownloadWorker
    {
        IDownloadConfig DownloadConfig { get; }


        Task<ulong> StartAsync(
            Uri resourceUri,
            Func<Memory<byte>, CancellationToken, Task> enqueueFn,
            Func<CancellationToken, Task<bool>> canEnqueuFn,
            CancellationToken token = default
        );
    }
}
