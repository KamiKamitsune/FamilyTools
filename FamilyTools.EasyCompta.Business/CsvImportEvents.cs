namespace FamilyTools.EasyCompta.Business;

/// <summary>
/// Échange fanout RabbitMQ sur lequel le worker diffuse la fin de traitement d'un import CSV
/// (une fois les écritures persistées). L'API s'y abonne pour notifier le front en temps réel.
/// </summary>
public static class CsvImportEvents
{
    public const string ExchangeName = "csv-import-events";
}
