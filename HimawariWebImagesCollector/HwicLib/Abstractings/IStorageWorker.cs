namespace Hwic.Abstractings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Pipes;


    public interface IStorageWorker
    {
        IStorageConfig Config { get; }


        // the image resource this worker store for
        Uri ResourceUri { get; }


        Task StoreAsync(
            IDataPipeConsumerEnd dataPipe,
            CancellationToken? optToken = null
        );
    }
}
