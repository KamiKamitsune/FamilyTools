using FamilyTools.EasyCompta.Business;
using RabbitMQ.Client;

namespace FamilyTools.EasyCompta.Services;

/// <summary>
/// Publie le contenu brut d'un CSV dans RabbitMQ (file durable, message persistant) pour
/// que le traitement soit fait de façon asynchrone et survive à un redémarrage de l'API.
/// </summary>
public sealed class CsvImportPublisher(IConnection connection) : ICsvImportPublisher
{
    private readonly IConnection _connection = connection;

    public async Task PublishAsync(byte[] csvFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(csvFile);

        await using var channel = await this._connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            CsvImportQueue.Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: CsvImportQueue.Name,
            mandatory: false,
            basicProperties: properties,
            body: csvFile,
            cancellationToken: cancellationToken);
    }
}
