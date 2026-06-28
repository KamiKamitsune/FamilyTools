using FamilyTools.EasyCompta.Business;
using FamilyTools.EasyCompta.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FamilyTools.EasyCompta.Services;

/// <summary>
/// S'abonne à l'échange fanout des fins d'import CSV (publié par le worker) et relaie
/// chaque événement aux clients SignalR connectés via <see cref="ImportHub"/>.
/// </summary>
public sealed class CsvImportEventsConsumer(
    IConnection connection,
    IHubContext<ImportHub> hub,
    ILogger<CsvImportEventsConsumer> logger) : BackgroundService
{
    private readonly IConnection _connection = connection;
    private readonly IHubContext<ImportHub> _hub = hub;
    private readonly ILogger<CsvImportEventsConsumer> _logger = logger;

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._channel = await this._connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await this._channel.ExchangeDeclareAsync(
            CsvImportEvents.ExchangeName,
            ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // File anonyme propre à cette instance d'API : chaque instance reçoit l'événement
        // pour notifier les clients qui lui sont connectés.
        var queue = await this._channel.QueueDeclareAsync(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: stoppingToken);

        await this._channel.QueueBindAsync(
            queue.QueueName,
            CsvImportEvents.ExchangeName,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(this._channel);
        consumer.ReceivedAsync += async (_, _) =>
        {
            this._logger.LogInformation("Import CSV terminé : notification des clients connectés.");
            await this._hub.Clients.All.SendAsync("importCompleted", stoppingToken);
        };

        await this._channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Arrêt normal du service.
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (this._channel is not null)
        {
            await this._channel.CloseAsync(cancellationToken);
            await this._channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
