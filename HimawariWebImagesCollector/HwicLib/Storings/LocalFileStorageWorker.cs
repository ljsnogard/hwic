namespace Hwic.Storings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;


    using Serilog;


    public class LocalFileStorageWorker : IStorageWorker
    {
        public LocalFileStorageConfig Config { get; }


        public Uri ResourceUri { get; }


        private ILogger Log { get; }


        IStorageConfig IStorageWorker.Config
            => this.Config;


        public LocalFileStorageWorker(
                LocalFileStorageConfig config,
                Uri resourceUri,
                ILogger logger)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
            this.Log = logger;
        }


        public async Task<ulong> StoreAsync(
                Func<CancellationToken, Task<Memory<byte>>> dequeueFn,
                Func<CancellationToken, Task<bool>> canDequeueFn,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var filePath = string.Empty;
            var writtenSize = 0UL;
            try
            {
                using var fileContStream = this.Config.CreateFileStream(this.ResourceUri);

                filePath = fileContStream.Name;
                this.Log.Information("Resource {@Uri} will store on local file {@Path}", this.ResourceUri, filePath);

                while (true)
                {
                    if (false == await canDequeueFn(token))
                        break;

                    var data = await dequeueFn(token);
                    await fileContStream.WriteAsync(data);
                    writtenSize += (uint)data.Length;

                    this.Log.Information("Resource {@Uri} store {@FileSize} bytes written to file {@Path}", this.ResourceUri, writtenSize, filePath);
                }
                return writtenSize;
            }
            catch (Exception e)
            {
                this.Log.Error("Exception occurred during store resource {@Uri} to local file: {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
        }
    }
}
