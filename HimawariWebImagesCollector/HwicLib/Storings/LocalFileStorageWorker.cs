namespace Hwic.Storings
{
    using System;
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Pipes;


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


        public async Task<uint> StoreAsync(
                IDataPipeConsumerEnd dataPipe,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var bc = 0u;
            var filePath = string.Empty;
            try
            {
                using var fileContStream = this.Config.CreateFileStream(this.ResourceUri);

                filePath = fileContStream.Name;
                this.Log.Information("Resource {@Uri} will store on local file {@Path}", this.ResourceUri, filePath);

                bc = await dataPipe.CopyToStreamAsync(fileContStream, token);
                return bc;
            }
            catch (Exception e)
            {
                this.Log.Error("Exception occurred during store resource {@Uri} to local file: {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
            finally
            {
                this.Log.Information($"{bc} bytes written from {this.ResourceUri} to file {filePath}.");
            }
        }
    }
}
