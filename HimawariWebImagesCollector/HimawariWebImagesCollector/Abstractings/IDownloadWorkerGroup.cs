namespace Hwic.Abstractings
{
    using System.Collections.Generic;


    public interface IDownloadWorkerGroup
    {
        IEnumerable<IDownloadWorker> Workers { get; }
    }
}
