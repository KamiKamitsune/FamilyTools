using FamilyTools.EasyCompta.IBusiness;

namespace FamilyTools.EasyCompta.Services;

public sealed class CSVConvertService(
    IBackgroundCSVConvert taskQueue,
    ILogger<CSVConvertService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ILogger<CSVConvertService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IBackgroundCSVConvert _taskQueue = taskQueue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("""
                                    {Name} is running.
                                    Tap W to add a work item to the 
                                    background queue.
                                    """,
            nameof(CSVConvertService));

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                var csvFiles = await this._taskQueue.DequeueAsync(stoppingToken);

                using var scope = this._scopeFactory.CreateScope();
                var importCsvBusiness = scope.ServiceProvider.GetRequiredService<IImportCSVBusiness>();
                await importCsvBusiness.CSVToAccountPages(csvFiles);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation(
            $"{nameof(CSVConvertService)} is stopping.");

        await base.StopAsync(stoppingToken);
    }
}