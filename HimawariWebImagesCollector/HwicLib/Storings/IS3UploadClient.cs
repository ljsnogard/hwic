namespace Hwic.Storings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    public interface IS3UploadClient : IDisposable
    {
        Task<ulong> UploadAsync(
            Stream inputStream,
            string bucketName,
            string key,
            CancellationToken? optToken = null
        );
    }
}
