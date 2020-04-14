namespace Hwic.Pipes
{
    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    public static class DataPipeStreamExtensions
    {
        public const uint DEFAULT_BUFF_SIZE = 1024;


        public static Task<uint> CopyToPipeAsync(
                this Stream stream,
                IDataPipeProducerEnd dataPipe,
                CancellationToken? optToken = null)
        {
            return stream.CopyToPipeAsync(
                dataPipe,
                DEFAULT_BUFF_SIZE,
                optToken
            );
        }


        public static Task<uint> CopyToStreamAsync(
                this IDataPipeConsumerEnd dataPipe,
                Stream stream,
                CancellationToken? optToken = null)
        {
            return dataPipe.CopyToStreamAsync(
                stream,
                DEFAULT_BUFF_SIZE,
                optToken
            );
        }


        public static async Task<uint> CopyToPipeAsync(
                this Stream          stream,
                IDataPipeProducerEnd producerEnd,
                uint                 bufferSize,
                CancellationToken?   optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var buff = new byte[bufferSize];
            var cp = 0u;
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var bc = await stream.ReadAsync(buff, 0, (int)bufferSize, token);
                    if (bc <= 0)
                        break;

                    cp += (uint)bc;
                    await producerEnd.WriteDataAsync(buff, 0u, (uint)bc);
                }
            }
            catch (TaskCanceledException)
            {
            }
            return cp;
        }


        public static async Task<uint> CopyToStreamAsync(
                this IDataPipeConsumerEnd consumerEnd,
                Stream                    stream,
                uint                      bufferSize,
                CancellationToken?        optToken = null)
        {
            var token = optToken.GetValueOrDefault(CancellationToken.None);
            var buff = new byte[bufferSize];
            var cp = 0u;
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var bc = await consumerEnd.ReadDataAsync(buff, 0, bufferSize, token);
                    if (bc == 0)
                        break;

                    cp += (uint)bc;
                    await stream.WriteAsync(buff, 0, (int)bc, token);
                }
            }
            catch (TaskCanceledException)
            {
            }
            return cp;
        }
    }
}
