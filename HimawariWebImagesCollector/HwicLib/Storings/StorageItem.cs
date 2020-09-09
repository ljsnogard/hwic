namespace Hwic.Storings
{
    using System;
    using System.Globalization;


    public sealed class StorageItem
    {
        public string Key { get; }


        public string ETag { get; }


        public ulong Size { get; }


        public bool IsDir { get; }


        public DateTimeOffset? LastModified { get; }


        internal StorageItem(Minio.DataModel.Item item)
        {
            DateTimeOffset? dt = null;
            if (!string.IsNullOrEmpty(item.LastModified))
            {
                dt = DateTimeOffset.Parse(
                    item.LastModified,
                    CultureInfo.InvariantCulture
                );
            }

            this.Key = item.Key;
            this.LastModified = dt;
            this.ETag = item.ETag;
            this.Size = item.Size;
            this.IsDir = item.IsDir;
        }
    }
}
