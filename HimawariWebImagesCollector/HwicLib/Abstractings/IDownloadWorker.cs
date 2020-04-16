namespace Hwic.Abstractings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    public interface IDownloadWorker
    {
        IDownloadConfig DownloadConfig { get; }


        Uri ResourceUri { get; }


        Task<ulong> StartAsync(
            Func<Memory<byte>, CancellationToken, Task> enqueueFn,
            Func<CancellationToken, Task<bool>> canEnqueuFn,
            CancellationToken? optToken
        );
    }
}
