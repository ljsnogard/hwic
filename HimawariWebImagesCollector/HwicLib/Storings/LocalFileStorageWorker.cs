namespace Hwic.Storings
{
    using System;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Loggings;


    public class LocalFileStorageWorker : IStorageWorker
    {
        public LocalFileStorageConfig Config { get; }


        IStorageConfig IStorageWorker.Config
            => this.Config;


        public LocalFileStorageWorker(LocalFileStorageConfig config)
            => this.Config = config;


        public async Task<ulong> StoreAsync(
                Uri fileSourceUri,
                Func<CancellationToken, Task<Memory<byte>>> dequeueFn,
                Func<CancellationToken, Task<bool>> canDequeueFn,
                CancellationToken token = default)
        {
            var log = this.GetLogger();

            var writtenSize = 0UL;
            try
            {
                using var fileContStream = this.Config.CreateFileStream(fileSourceUri);

                var filePath = fileContStream.Name;
                log.Here().Information("Resource {@Uri} will store on local file {@Path}", fileSourceUri, filePath);

                while (true)
                {
                    if (false == await canDequeueFn(token))
                        break;

                    var data = await dequeueFn(token);
                    await fileContStream.WriteAsync(data);
                    writtenSize += (uint)data.Length;
                }
                log.Here().Information("Resource {@Uri} store {@FileSize} bytes written to file {@Path}", fileSourceUri, writtenSize, filePath);
                return writtenSize;
            }
            catch (Exception e)
            {
                log.Here().Error("Exception occurred during store resource {@Uri} to local file: {@ErrorMessage}", fileSourceUri, e.Message);
                throw;
            }
        }
    }
}
