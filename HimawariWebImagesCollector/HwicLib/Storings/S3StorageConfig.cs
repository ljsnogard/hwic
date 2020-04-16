namespace Hwic.Storings
{
    using System;

    using System.Collections.Generic;
    using System.Collections.Specialized;

    using System.IO;
    using System.Linq;


    using Hwic.Abstractings;


    using Serilog;


    public class S3StorageConfig : IStorageConfig
    {
        public readonly string BucketName;


        public S3StorageConfig(string bucketName)
        {
            this.BucketName = bucketName;
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
