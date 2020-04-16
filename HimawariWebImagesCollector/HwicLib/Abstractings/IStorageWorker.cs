namespace Hwic.Abstractings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    public interface IStorageWorker
    {
        IStorageConfig Config { get; }


        // the image resource this worker store for
        Uri ResourceUri { get; }


        Task<ulong> StoreAsync(
            Func<CancellationToken, Task<Memory<byte>>> dequeueFn,
            Func<CancellationToken, Task<bool>> canDequeueFn,
            CancellationToken? optToken = null
        );
    }
}
