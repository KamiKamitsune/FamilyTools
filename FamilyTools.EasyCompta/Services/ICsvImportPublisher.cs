namespace FamilyTools.EasyCompta.Services;

/// <summary>Publie le contenu d'un fichier CSV importé vers la file de traitement.</summary>
public interface ICsvImportPublisher
{
    Task PublishAsync(byte[] csvFile, CancellationToken cancellationToken = default);
}
