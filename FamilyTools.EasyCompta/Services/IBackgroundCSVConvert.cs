namespace FamilyTools.EasyCompta.Services
{
    public interface IBackgroundCSVConvert
    {
        ValueTask AddWorkItemInQueueBackgroundAsync(byte[] csvFiles);

        ValueTask<byte[]> DequeueAsync(CancellationToken cancellationToken);
    }
}

