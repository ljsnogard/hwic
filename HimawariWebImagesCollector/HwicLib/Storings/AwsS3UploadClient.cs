namespace Hwic.Storings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using Amazon.S3;
    using Amazon.S3.Transfer;


    public class AwsS3UploadClient : IS3UploadClient
    {
        private readonly AmazonS3Client s3Client_;


        public AwsS3UploadClient(AmazonS3Client s3Client)
            => this.s3Client_ = s3Client;


        public async Task<ulong> UploadAsync(
                Stream inputStream,
                string bucketName,
                string key,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            using var transfer = new TransferUtility(this.s3Client_);

            var before = inputStream.CanSeek ? inputStream.Position : 0L;

            await transfer.UploadAsync(
                stream: inputStream,
                bucketName: bucketName,
                key: key,
                cancellationToken: token
            );
            var after = inputStream.CanSeek ? inputStream.Position : 0L;
            return (ulong)(after - before);
        }


        public void Dispose()
            => this.s3Client_.Dispose();
    }
}
