namespace Hwic.Pipes
{
    using System.Threading;
    using System.Threading.Tasks;


    public interface IDataPipeProducerEnd
    {
        ValueTask<bool> CloseAsync();


        ValueTask WriteDataAsync(
            byte[] buffer,
            uint   offset,
            uint   length,
            CancellationToken? cancelWriteToken = null
        );
    }
}
