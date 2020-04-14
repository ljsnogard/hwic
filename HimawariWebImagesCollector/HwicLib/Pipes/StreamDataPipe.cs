namespace Hwic.Pipes
{
    using System;
    
    using System.Collections.Specialized;

    using System.IO;

    using System.Threading;
    using System.Threading.Tasks;


    using Hwic.Pipes;


    using Serilog;


    public static class StreamDataPipe
    {
        public static void DeferStream<TStream>(TStream stream) where TStream : Stream
        {
            stream.Close();
            stream.Dispose();
        }


        public static StreamDataPipe<TStream> Create<TStream>() where TStream : Stream, new()
        {
            var stream = new TStream();
            return StreamDataPipe<TStream>.Create(
                stream,
                StreamDataPipe.DeferStream
            );
        }


        public static StreamDataPipe<TStream> Create<TStream>(Func<TStream> createStream) where TStream : Stream
        {
            var stream = createStream();
            return StreamDataPipe<TStream>.Create(
                stream,
                StreamDataPipe.DeferStream
            );
        }


        public static StreamDataPipe<TStream> Create<TStream>(
                Func<TStream> createStream,
                Action<TStream> deferStream)
            where TStream : Stream
        {
            var stream = createStream();
            return StreamDataPipe<TStream>.Create(
                stream,
                deferStream
            );
        }


        public static async Task<StreamDataPipe<TStream>> CreateAsync<TStream>(
                Func<Task<TStream>> createStream,
                Action<TStream> deferStream)
            where TStream : Stream
        {
            var stream = await createStream();
            return StreamDataPipe<TStream>.Create(
                stream,
                deferStream
            );
        }
    }



    public sealed class StreamDataPipe<TStream> : IDataPipeConsumerEnd, IDataPipeProducerEnd
        where TStream : Stream
    {
        private readonly SemaphoreSlim semaphore_;


        private readonly TStream stream_;


        private readonly Action<TStream> releaseStream_;


        private const int PRODUCER_FLAG = 0;
        private const int CONSUMER_FLAG = 1;
        private const int DISPOSED_FLAG = 2;

        private BitVector32 flags_;


        private StreamDataPipe(
                in TStream stream,
                Action<TStream> releaseStream)
        {
            this.semaphore_ = new SemaphoreSlim(1);
            this.stream_ = stream;
            this.flags_ = new BitVector32(0);
            this.releaseStream_ = releaseStream;
        }


        private void CheckDestroy_()
        {
            if (this.flags_[DISPOSED_FLAG])
                return;

            if (this.flags_[PRODUCER_FLAG] && this.flags_[CONSUMER_FLAG])
            {
                this.releaseStream_(this.stream_);
                this.flags_[DISPOSED_FLAG] = true;
            }
        }


        public static StreamDataPipe<TStream> Create(TStream stream, Action<TStream> deferStream)
            => new StreamDataPipe<TStream>(stream, deferStream);


        async ValueTask<bool> IDataPipeProducerEnd.CloseAsync()
        {
            await this.semaphore_.WaitAsync();
            try
            {
                if (this.flags_[PRODUCER_FLAG])
                    return false;

                this.flags_[PRODUCER_FLAG] = true;
                return true;
            }
            finally
            {
                this.CheckDestroy_();
                this.semaphore_.Release();
            }
        }


        async ValueTask<bool> IDataPipeConsumerEnd.CloseAsync()
        {
            await this.semaphore_.WaitAsync();
            try
            {
                if (this.flags_[CONSUMER_FLAG])
                    return false;

                this.flags_[CONSUMER_FLAG] = true;
                return true;
            }
            finally
            {
                this.CheckDestroy_();
                this.semaphore_.Release();
            }
        }


        async ValueTask<bool> IDataPipeConsumerEnd.HasProducerClosedAsync()
        {
            await this.semaphore_.WaitAsync();
            try
            {
                return this.flags_[PRODUCER_FLAG];
            }
            finally
            {
                this.semaphore_.Release();
            }
        }


        async ValueTask<uint> IDataPipeConsumerEnd.ReadDataAsync(
                byte[] buffer,
                uint   offset,
                uint   length,
                CancellationToken? cancelReadToken)
        {
            var token = cancelReadToken.GetValueOrDefault(CancellationToken.None);
            await this.semaphore_.WaitAsync(token);
            var bc = 0u;
            try
            {
                while (bc < length)
                {
                    var rc = await this.stream_.ReadAsync(
                        buffer,
                        (int)offset,
                        (int)length,
                        token
                    );
                    if (rc >= 0)
                        bc += (uint)rc;
                    else
                        break;
                }
                return bc;
            }
            catch (TaskCanceledException)
            {
                return bc;
            }
            catch (Exception e)
            {
                var log = Log.Logger;
                log.Error(e.Message);
                throw;
            }
            finally
            {
                this.semaphore_.Release();
            }
        }


        async ValueTask IDataPipeProducerEnd.WriteDataAsync(
                byte[] buffer,
                uint offset,
                uint length,
                CancellationToken? cancelWriteToken)
        {
            var token = cancelWriteToken.GetValueOrDefault(CancellationToken.None);
            await this.semaphore_.WaitAsync(token);
            try
            {
                await this.stream_.WriteAsync(
                    buffer,
                    (int)offset,
                    (int)length,
                    token
                );
            }
            catch (Exception e)
            {
                var log = Log.Logger;
                log.Error(e.Message);
                throw;
            }
            finally
            {
                this.semaphore_.Release();
            }
        }
    }
}
