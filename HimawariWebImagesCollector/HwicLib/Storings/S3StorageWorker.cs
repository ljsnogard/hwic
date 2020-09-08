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


        public S3StorageWorker(S3StorageConfig config)
            => this.Config = config;


        IStorageConfig IStorageWorker.Config
            => this.Config;


        public async Task<ulong> StoreAsync(
                Uri fileSourceUri,
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                CancellationToken token = default)
        {
            var log = this.GetLogger();

            var uploadSize = 0UL;
            try
            {
                using var s3Client = this.Config.GetUploadClient();

                using var senderPipe = new AnonymousPipeServerStream(PipeDirection.Out);
                using var receiverPipe = new AnonymousPipeClientStream(
                    PipeDirection.In,
                    senderPipe.GetClientHandleAsString()
                );
                var writerTask = this.WriteToStreamAsync_(
                    fileSourceUri,
                    dequeue,
                    canDequeue,
                    senderPipe,
                    token
                );
                var uploadTask = s3Client.UploadAsync(
                    inputStream: receiverPipe,
                    bucketName: this.Config.BucketName,
                    key: this.Config.GenerateFilePath(fileSourceUri),
                    token: token
                );
                uploadSize = await writerTask;
                senderPipe.WaitForPipeDrain();

                await uploadTask;
                return uploadSize;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Upload resource {@Uri} cancelled after {@Bytes} bytes uploaded.", fileSourceUri, uploadSize);
                return uploadSize;
            }
            catch (Exception e)
            {
                log.Here().Error(
                    "Exception occurred when uploading stored resource {@Uri}: {@ErrorMessage}",
                    fileSourceUri, e.Message
                );
                throw;
            }
        }


        private async Task<ulong> WriteToStreamAsync_(
                Uri fileSourceUri,
                Func<CancellationToken, Task<Memory<byte>>> dequeue,
                Func<CancellationToken, Task<bool>> canDequeue,
                Stream stream,
                CancellationToken token = default)
        {
            var log = this.GetLogger();

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
                log.Here().Error("IOException occurred when writing to S3 upload stream for {@Uri}: {@ErrorMessage}", fileSourceUri, e.Message);
                throw;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Cancelled writing to S3 upload stream for {@Uri}", fileSourceUri);
            }
            return wc;
        }
    }
}
