namespace Hwic.Storings
{
    using System;
    using System.Collections.Generic;

    using System.Threading;
    using System.Threading.Tasks;


    using Minio;


    public sealed class S3ObjectsListingClient
    {
        private readonly MinioClient client_;


        internal S3ObjectsListingClient(MinioClient client)
            => this.client_ = client;


        public async Task<LinkedList<StorageItem>> GetObjectListAsync(
                string bucketName,
                CancellationToken token = default)
        {
            var observableItems = this.client_.ListObjectsAsync(
                bucketName: bucketName,
                prefix    : null,
                recursive : false,
                cancellationToken: token
            );
            var mutex = new object();
            var completionSignal = new TaskCompletionSource<object>();
            var itemList = new LinkedList<StorageItem>();

            using var subscription = observableItems.Subscribe(
                onNext     : HandleItems_,
                onError    : HandleException_,
                onCompleted: HandleCompleted_
            );
            var res = await Task.Run(() => completionSignal.Task, token);
            return res switch
            {
                LinkedList<StorageItem> resultList => resultList,
                Exception exception => throw exception,
                _ => throw new Exception($"Unexpected object type: {res.GetType()}"),
            };

            void HandleItems_(Minio.DataModel.Item item)
            {
                if (item.IsDir)
                    return;

                var storageItem = new StorageItem(item);
                lock (mutex)
                    itemList.AddLast(storageItem);
            }

            void HandleException_(Exception ex)
            {
                lock (mutex)
                    completionSignal.TrySetResult(ex);
            }

            void HandleCompleted_()
            {
                lock (mutex)
                    completionSignal.TrySetResult(itemList);
            }
        }
    }
}
