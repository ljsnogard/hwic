namespace Hwic.Abstractings
{
    using System;


    public interface IDownloadWorkerGroupProvider
    {
        IDownloadWorkerGroup GetDownloadWorkerGroup(
            in IDownloadConfig downloadConfig,
            in DateTimeOffset imageTime
        );
    }
}
