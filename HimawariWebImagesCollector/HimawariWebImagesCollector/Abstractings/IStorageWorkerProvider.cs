namespace Hwic.Abstractings
{
    public interface IStorageWorkerProvider
    {
        IStorageWorker GetStorageWorker(in IStorageConfig config);
    }
}
