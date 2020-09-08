namespace Hwic.Storings
{
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using Minio;


    public readonly struct MinioS3UploadClient : IS3UploadClient
    {
        private readonly MinioClient client_;


        public MinioS3UploadClient(MinioClient client)
            => this.client_ = client;


        public async Task<ulong> UploadAsync(
                Stream inputStream,
                string bucketName,
                string key,
                CancellationToken token = default)
        {
            if (false == await this.client_.BucketExistsAsync(bucketName, token))
                throw new System.Exception($"Bucket ({bucketName}) not found");

            using var memstream = new MemoryStream();
            await inputStream.CopyToAsync(memstream);

            var length = (ulong)memstream.Length;
            memstream.Position = 0L;

            await this.client_.PutObjectAsync(
                bucketName: bucketName,
                objectName: key,
                data: inputStream,
                size: memstream.Length,
                contentType: null,
                metaData: null,
                sse: null,
                cancellationToken: token
            );
            return length;
        }


        public void Dispose()
        { }
    }
}
