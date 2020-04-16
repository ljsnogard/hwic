namespace Hwic.Storings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    public struct S3ApiStorageClient : IDisposable
    {
        public readonly Task<ulong> UploadAsync(
                Stream inputStream,
                string bucketName,
                string key,
                CancellationToken? optToken = null)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
