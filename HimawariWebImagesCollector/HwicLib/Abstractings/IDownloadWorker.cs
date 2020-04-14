namespace Hwic.Abstractings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Pipes;


    public interface IDownloadWorker
    {
        IDownloadConfig DownloadConfig { get; }


        Uri ResourceUri { get; }


        Task<uint> StartAsync(
            IDataPipeProducerEnd dataPipe,
            CancellationToken? optToken
        );
    }
}
