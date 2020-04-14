namespace Hwic.Net
{
    using System;

    using System.Net.Http;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Pipes;


    using Serilog;


    public class HttpClientDownloadWorker : IDownloadWorker
    {
        public HttpDownloadConfig Config { get; }


        public Uri ResourceUri { get; }


        private ILogger Log { get; }


        IDownloadConfig IDownloadWorker.DownloadConfig
            => this.Config;


        public HttpClientDownloadWorker(
                HttpDownloadConfig config,
                Uri resourceUri,
                ILogger logger)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
            this.Log = logger;
        }


        public async Task<uint> StartAsync(
                IDataPipeProducerEnd dataPipe,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);

            var httpClient = this.Config.GetHttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, this.ResourceUri);
            var sendRequestTask = httpClient.SendAsync(request, token);

            this.Log.Information("Requesting {@Uri}", this.ResourceUri);
            var bc = 0u;
            try
            {
                using var respMsg = await sendRequestTask;
                this.Log.Information("Got {@Uri} response {@Status}", this.ResourceUri, respMsg.StatusCode);

                using var httpContStream = await respMsg.Content.ReadAsStreamAsync();
                bc = await httpContStream.CopyToPipeAsync(dataPipe, token);
                return bc;
            }
            catch (TaskCanceledException)
            {
                this.Log.Warning("Cancelled when downloading from {@Uri}", this.ResourceUri);
                return 0u;
            }
            catch (Exception e)
            {
                this.Log.Error("Exception occurred when downloading from {@Uri}. {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
            finally
            {
                this.Log.Information($"{bc} bytes downloaded from: {this.ResourceUri}");
            }
        }
    }
}
