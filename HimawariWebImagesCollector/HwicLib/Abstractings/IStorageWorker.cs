namespace Hwic.Abstractings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    public interface IStorageWorker
    {
        IStorageConfig Config { get; }


        Task<ulong> StoreAsync(
            Uri fileSourceUri,
            Func<CancellationToken, Task<Memory<byte>>> dequeueFn,
            Func<CancellationToken, Task<bool>> canDequeueFn,
            CancellationToken token = default
        );
    }
}
