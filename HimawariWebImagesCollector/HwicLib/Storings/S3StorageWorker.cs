namespace Hwic.Storings
{
    using System;

    using System.IO;
    using System.IO.Pipes;

    using System.Threading;
    using System.Threading.Tasks;


    using Amazon.S3;
    using Amazon.S3.Transfer;


    using Hwic.Abstractings;


    using Serilog;


    public class S3StorageWorker : IStorageWorker
    {
        public S3StorageConfig Config { get; }


        public Uri ResourceUri { get; }


        private ILogger Log { get; }


        public S3StorageWorker(S3StorageConfig config, Uri resourceUri, ILogger logger)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
            this.Log = logger;
        }


        IStorageConfig IStorageWorker.Config
            => this.Config;


        public async Task<ulong> StoreAsync(
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var uploadSize = 0UL;
            try
            {
                using var s3Client = this.Config.CreateClient();
                using var fileTransferUtility = new TransferUtility(s3Client);

                using var senderPipe = new AnonymousPipeServerStream(PipeDirection.Out);
                using var receiverPipe = new AnonymousPipeClientStream(
                    PipeDirection.In,
                    senderPipe.GetClientHandleAsString()
                );
                var writerTask = this.WriteToStreamAsync_(dequeue, canDequeue, senderPipe, token);

                var uploadTask = fileTransferUtility.UploadAsync(
                    stream: receiverPipe,
                    bucketName: this.Config.BucketName,
                    key: this.Config.GenerateFilePath(this.ResourceUri),
                    cancellationToken: token
                );

                uploadSize = await writerTask;
                senderPipe.WaitForPipeDrain();

                await uploadTask;
                return uploadSize;
            }
            catch (TaskCanceledException)
            {
                return uploadSize;
            }
            catch (AmazonS3Exception s3Exception)
            {
                this.Log.Error(
                    "AmazonS3Exception occurred when writing to S3 upload stream for {@Uri}: {@ErrorMessage}",
                    this.ResourceUri, s3Exception.Message
                );
                throw;
            }
        }


        private async Task<ulong> WriteToStreamAsync_(
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                Stream stream,
                CancellationToken token)
        {
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
                this.Log.Error("IOException occurred when writing to S3 upload stream for {@Uri}: {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
            catch (TaskCanceledException)
            {
                this.Log.Warning("Cancelled writing to S3 upload stream for {@Uri}", this.ResourceUri);
            }
            return wc;
        }
    }
}
