namespace Hwic.Pipes
{
    using System.Threading;
    using System.Threading.Tasks;


    public interface IDataPipeConsumerEnd
    {
        ValueTask<bool> HasProducerClosedAsync();


        ValueTask<bool> CloseAsync();


        ValueTask<uint> ReadDataAsync(
            byte[] buffer,
            uint   offset,
            uint   length,
            CancellationToken? cancelReadToken = null
        );
    }
}
