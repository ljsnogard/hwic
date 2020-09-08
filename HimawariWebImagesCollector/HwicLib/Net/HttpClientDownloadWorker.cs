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



        IDownloadConfig IDownloadWorker.DownloadConfig
            => this.Config;


        public HttpClientDownloadWorker(HttpDownloadConfig config)
            => this.Config = config;


        public async Task<ulong> StartAsync(
                Uri resourceUri,
                Func<Memory<byte>, CancellationToken, Task> enqueue,
                Func<CancellationToken, Task<bool>> canEnqueue,
                CancellationToken token = default)
        {
            var log = this.GetLogger();

            var bc = 0uL;
            try
            {
                var httpClient = this.Config.GetHttpClient();

                using var request = new HttpRequestMessage(HttpMethod.Get, resourceUri);
                var sendRequestTask = httpClient.SendAsync(request, token);

                log.Here().Verbose("Requesting {@Uri}", resourceUri);

                using var respMsg = await sendRequestTask;
                if (respMsg.IsSuccessStatusCode)
                    log.Here().Verbose("Got {@Uri} response {@Status}", resourceUri, respMsg.StatusCode);
                else
                    log.Here().Warning("Got {@Uri} response {@Status}", resourceUri, respMsg.StatusCode);

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
                        if (rc < 0)
                            log.Here().Warning("Download {@Uri} readAsync returns unexpected {@ReadByteCount}", resourceUri, rc);
                        break;
                    }
                }
                return bc;
            }
            catch (TaskCanceledException)
            {
                log.Here().Warning("Cancelled when downloading from {@Uri}", resourceUri);
                return bc;
            }
            catch (Exception e)
            {
                log.Here().Error("Exception occurred when downloading from {@Uri}. {@ErrorMessage}", resourceUri, e.Message);
                throw;
            }
            finally
            {
                log.Here().Information($"{bc} bytes downloaded from: {resourceUri}");
            }
        }
    }
}
