using System.Text.Json;
using FamilyTools.EasyCompta.Business;
using FamilyTools.EasyCompta.IBusiness;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FamilyTools.CsvWorker;

/// <summary>
/// Consomme la file RabbitMQ des imports CSV et déclenche leur traitement
/// (parsing + persistance) un message à la fois.
/// </summary>
public sealed class CsvImportConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<CsvImportConsumer> logger) : BackgroundService
{
    private readonly IConnection _connection = connection;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<CsvImportConsumer> _logger = logger;

    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._stoppingToken = stoppingToken;
        this._channel = await this._connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await this._channel.QueueDeclareAsync(
            CsvImportQueue.Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Le traitement (parsing + EF) est lourd : on ne préfetch qu'un message à la fois.
        await this._channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(this._channel);
        consumer.ReceivedAsync += this.OnMessageReceivedAsync;

        await this._channel.BasicConsumeAsync(
            CsvImportQueue.Name,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        this._logger.LogInformation("Consommateur d'imports CSV démarré (file {Queue}).", CsvImportQueue.Name);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Arrêt normal du service.
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        try
        {
            var csvFile = eventArgs.Body.ToArray();

            using var scope = this._scopeFactory.CreateScope();
            var importBusiness = scope.ServiceProvider.GetRequiredService<IImportCSVBusiness>();
            await importBusiness.CSVToAccountPages(csvFile);

            await this._channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);

            // Import persisté : on diffuse l'événement de fin pour que l'API rafraîchisse le front.
            await this.PublishCompletedAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Échec du traitement d'un import CSV.");
            // requeue: false -> on évite une boucle infinie sur un message empoisonné.
            await this._channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task PublishCompletedAsync()
    {
        // Canal court dédié : on ne mêle pas publication et consommation sur le même canal.
        await using var channel = await this._connection.CreateChannelAsync(cancellationToken: this._stoppingToken);
        await channel.ExchangeDeclareAsync(
            CsvImportEvents.ExchangeName,
            ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            cancellationToken: this._stoppingToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(new { completedAtUtc = DateTime.UtcNow });
        await channel.BasicPublishAsync(
            exchange: CsvImportEvents.ExchangeName,
            routingKey: string.Empty,
            body: body,
            cancellationToken: this._stoppingToken);
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
