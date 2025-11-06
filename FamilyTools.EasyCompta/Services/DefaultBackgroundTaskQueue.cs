using System.Threading.Channels;

namespace FamilyTools.EasyCompta.Services
{
    public class DefaultBackgroundTaskQueue : IBackgroundCSVConvert
    {
        private readonly Channel<byte[]> _queue;

        public DefaultBackgroundTaskQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<byte[]>(options);
        }

        public async ValueTask AddWorkItemInQueueBackgroundAsync(byte[] csvFiles)
        {
            ArgumentNullException.ThrowIfNull(csvFiles);

            await _queue.Writer.WriteAsync(csvFiles);
        }

        public async ValueTask<byte[]> DequeueAsync(CancellationToken cancellationToken)
        {
            var csvFiles = await _queue.Reader.ReadAsync(cancellationToken);

            return csvFiles;
        }
    }
}
