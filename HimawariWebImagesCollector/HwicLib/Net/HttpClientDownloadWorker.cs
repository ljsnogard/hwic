namespace Hwic.Net
{
    using System;

    using System.Net.Http;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Abstractings;
    using Hwic.Loggings;


    public class HttpClientDownloadWorker : IDownloadWorker
    {
        public HttpDownloadConfig Config { get; }


        public Uri ResourceUri { get; }


        IDownloadConfig IDownloadWorker.DownloadConfig
            => this.Config;


        public HttpClientDownloadWorker(HttpDownloadConfig config, Uri resourceUri)
        {
            this.Config = config;
            this.ResourceUri = resourceUri;
        }


        public async Task<ulong> StartAsync(
                Func<Memory<byte>, CancellationToken, Task> enqueue,
                Func<CancellationToken, Task<bool>> canEnqueue,
                CancellationToken? optToken = null)
        {
            var log = this.GetLogger();

            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var bc = 0uL;
            try
            {
                var httpClient = this.Config.GetHttpClient();

                using var request = new HttpRequestMessage(HttpMethod.Get, this.ResourceUri);
                var sendRequestTask = httpClient.SendAsync(request, token);

                log.Here().Verbose("Requesting {@Uri}", this.ResourceUri);

                using var respMsg = await sendRequestTask;
                if (respMsg.IsSuccessStatusCode)
                    log.Here().Verbose("Got {@Uri} response {@Status}", this.ResourceUri, respMsg.StatusCode);
                else
                    log.Here().Warning("Got {@Uri} response {@Status}", this.ResourceUri, respMsg.StatusCode);

                using var httpContStream = await respMsg.Content.ReadAsStreamAsync();

                var buffer = new byte[this.Config.BufferSize];
                while (true)
                {
                    if (false == await canEnqueue(token))
                    {
                        log.Here().Warning($"canEnqueue returns false");
                        break;
                    }
                    var rc = await httpContStream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        token
                    );
                    if (rc > 0)
                    {
                        var msg = new byte[rc];
                        Array.Copy(buffer, msg, rc);
                        await enqueue(msg, token);

                        bc += (uint)rc;
                    }
                    if (rc <= 0)
                    {
                        log.Here().Verbose("Download {@Uri} readAsync returns {rc}", rc);
                        break;
                    }
                }
                return bc;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Cancelled when downloading from {@Uri}", this.ResourceUri);
                return bc;
            }
            catch (Exception e)
            {
                log.Here().Error("Exception occurred when downloading from {@Uri}. {@ErrorMessage}", this.ResourceUri, e.Message);
                throw;
            }
            finally
            {
                log.Here().Information($"{bc} bytes downloaded from: {this.ResourceUri}");
            }
        }
    }
}
