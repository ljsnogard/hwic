namespace Hwic.Storings
{
    using System.Linq;


    public static class S3StorageConfigGetClientExtensions
    {
        public static IS3UploadClient GetUploadClient(this S3StorageConfig s3Config)
        {
            var minioClient = new Minio.MinioClient(
                s3Config.Provider.EndPoint,
                s3Config.AccessKey,
                s3Config.SecretKey
            );
            if (s3Config.Proxies.Any())
            {
                var proxyInfo = s3Config.Proxies.First();
                minioClient.WithProxy(proxyInfo.CreateWebProxy());
            }
            return new MinioS3UploadClient(minioClient);
        }
    }
}
