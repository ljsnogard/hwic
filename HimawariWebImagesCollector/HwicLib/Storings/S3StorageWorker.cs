namespace Hwic.Storings
{
    using System;

    using System.IO;
    using System.IO.Pipes;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Loggings;


    public class S3StorageWorker : IStorageWorker
    {
        public S3StorageConfig Config { get; }


        public Uri ResourceUri { get; }


        public S3StorageWorker(S3StorageConfig config, Uri resourceUri)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
        }


        IStorageConfig IStorageWorker.Config
            => this.Config;


        public async Task<ulong> StoreAsync(
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                CancellationToken? optToken = null)
        {
            var log = this.GetLogger();

            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var uploadSize = 0UL;
            try
            {
                using var s3Client = this.Config.CreateUploadClient();

                using var senderPipe = new AnonymousPipeServerStream(PipeDirection.Out);
                using var receiverPipe = new AnonymousPipeClientStream(
                    PipeDirection.In,
                    senderPipe.GetClientHandleAsString()
                );
                var writerTask = this.WriteToStreamAsync_(
                    dequeue,
                    canDequeue,
                    senderPipe,
                    optToken
                );
                var uploadTask = s3Client.UploadAsync(
                    inputStream: receiverPipe,
                    bucketName: this.Config.BucketName,
                    key: this.Config.GenerateFilePath(this.ResourceUri),
                    optToken: optToken
                );
                uploadSize = await writerTask;
                senderPipe.WaitForPipeDrain();

                await uploadTask;
                return uploadSize;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Upload resource {@Uri} cancelled after {@Bytes} bytes uploaded.", this.ResourceUri, uploadSize);
                return uploadSize;
            }
            catch (Exception e)
            {
                log.Here().Error(
                    "Exception occurred when uploading stored resource {@Uri}: {@ErrorMessage}",
                    this.ResourceUri, e.Message
                );
                throw;
            }
        }


        private async Task<ulong> WriteToStreamAsync_(
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                Stream stream,
                CancellationToken? optToken = null)
        {
            var log = this.GetLogger();

            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var wc = 0UL;
            try
            {
                while (true)
                {
                    if (false == await canDequeue(token))
                        break;

                    var data = await dequeue(token);
                    stream.Write(data.ToArray(), 0, data.Length);
                    wc += (uint)data.Length;
                }
                return wc;
            }
            catch (IOException e)
            {
                log.Here().Error("IOException occurred when writing to S3 upload stream for {@Uri}: {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Cancelled writing to S3 upload stream for {@Uri}", this.ResourceUri);
            }
            return wc;
        }
    }
}
