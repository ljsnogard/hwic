namespace Hwic.Storings
{
    using System;

    using System.Collections.Generic;


    using Hwic.Abstractings;
    using Hwic.Net;


    public abstract class S3StorageConfig : IStorageConfig
    {
        public S3Provider Provider { get; }


        public string BucketName { get; }


        public string AccessKey { get; }


        public string SecretKey { get; }


        public IEnumerable<Socks5ProxyInfo> Proxies { get; }


        protected S3StorageConfig(
                S3Provider provider,
                string bucketName,
                string accessKey,
                string secretKey,
                IEnumerable<Socks5ProxyInfo> proxies)
        {
            this.Provider = provider;
            this.BucketName = bucketName;
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.Proxies = proxies;
        }


        /// <summary>
        /// Generate a key for S3 upload. Which is also the path when navigate.
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public string GenerateFilePath(Uri resourceUri)
            => resourceUri.AbsolutePath;
    }
}
