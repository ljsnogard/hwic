namespace Hwic.Net
{
    using System;

    using System.Net.Http;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Pipes;


    using Serilog;


    public class HttpClientDownloader : IDownloadWorker
    {
        public HttpDownloadConfig Config { get; }


        public Uri ResourceUri { get; }


        public ILogger Log { get; }


        IDownloadConfig IDownloadWorker.DownloadConfig
            => this.Config;


        public HttpClientDownloader(
                HttpDownloadConfig config,
                Uri resourceUri,
                ILogger logger)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
            this.Log = logger;
        }


        public async Task StartAsync(
                IDataPipeProducerEnd dataPipe,
                CancellationToken? optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);

            var httpClient = this.Config.GetHttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, this.ResourceUri);
            var sendRequestTask = httpClient.SendAsync(request, token);

            this.Log.Verbose("Requesting {@Uri}", this.ResourceUri);
            try
            {
                using var respMsg = await sendRequestTask;
                this.Log.Verbose("Got {@Uri} response headers: {@Headers}", this.ResourceUri, respMsg.Headers);

                using var httpContStream = await respMsg.Content.ReadAsStreamAsync();
                await httpContStream.CopyToPipeAsync(dataPipe, token);
            }
            catch (TaskCanceledException)
            {
                this.Log.Warning("Cancelled when downloading from {@Uri}", this.ResourceUri);
            }
            catch (Exception e)
            {
                this.Log.Error("Exception occurred when downloading from {@Uri}. {@ErrorMessage}", this.ResourceUri, e.Message);
            }
        }
    }
}
