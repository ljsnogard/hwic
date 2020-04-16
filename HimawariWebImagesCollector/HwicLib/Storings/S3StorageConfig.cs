namespace Hwic.Storings
{
    using System;

    using System.Collections.Generic;
    using System.Collections.Specialized;

    using System.IO;
    using System.Linq;


    using Hwic.Abstractings;
    using Hwic.Net;


    using Serilog;


    public abstract class S3StorageConfig : IStorageConfig
    {
        public string BucketName { get; }


        public string AccessKey { get; }


        public string SecretKey { get; }


        public Uri ServiceUri { get; }


        public IEnumerable<Socks5ProxyInfo> Proxies { get; }


        protected S3StorageConfig(
                string bucketName,
                string accessKey,
                string secretKey,
                Uri serviceUri,
                IEnumerable<Socks5ProxyInfo> proxies)
        {
            this.BucketName = bucketName;
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.ServiceUri = serviceUri;
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
