namespace Hwic.Storings
{
    using System.Collections.Generic;
    using System.Linq;


    using Hwic.Utils;


    using Minio;


    public static class S3StorageConfigGetClientExtensions
    {
        private static readonly Dictionary<S3Provider, MinioClient> clientDict_ =
            new Dictionary<S3Provider, MinioClient>();


        private static readonly SyncReaderWriterLock rwlock_ =
            new SyncReaderWriterLock();


        private static MinioClient CreateMiniotClient_(S3StorageConfig s3Config)
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
            clientDict_.Add(s3Config.Provider, minioClient);

            #if RELEASE

            return minioClient.WithSSL();

            #else

            return minioClient;

            #endif
        }


        public static IS3UploadClient GetUploadClient(this S3StorageConfig s3Config)
        {
            return rwlock_.EnterUpgradableLock(upg =>
            {
                if (clientDict_.TryGetValue(s3Config.Provider, out var minioClient))
                    return new MinioS3UploadClient(minioClient);
                else
                    return upg.Enter(CreateClient_);
            });


            IS3UploadClient CreateClient_()
            {
                var minioClient = CreateMiniotClient_(s3Config);
                return new MinioS3UploadClient(minioClient);
            }
        }


        public static S3ObjectsListingClient GetQueryClient(this S3StorageConfig s3Config)
        {
            return rwlock_.EnterUpgradableLock(upg =>
            {
                if (clientDict_.TryGetValue(s3Config.Provider, out var minioClient))
                    return new S3ObjectsListingClient(minioClient);
                else
                    return upg.Enter(CreateClient_);
            });


            S3ObjectsListingClient CreateClient_()
            {
                var minioClient = CreateMiniotClient_(s3Config);
                return new S3ObjectsListingClient(minioClient);
            }
        }
    }
}
