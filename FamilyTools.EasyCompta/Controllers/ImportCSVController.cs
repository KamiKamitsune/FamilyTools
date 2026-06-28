using FamilyTools.EasyCompta.Services;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class ImportCSVController(ICsvImportPublisher publisher, ILogger<ImportCSVController> logger) : ControllerBase
{
    private const long MaxCsvSizeBytes = 5 * 1024 * 1024; // 5 Mo
    private static readonly string[] AllowedExtensions = [".csv"];

    private readonly ICsvImportPublisher publisher = publisher;
    private readonly ILogger<ImportCSVController> logger = logger;

    [Route("[action]")]
    [Route("")]
    [HttpPost]
    [RequestSizeLimit(MaxCsvSizeBytes)]
    public async Task<ActionResult> ImportCSV(IFormFile csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
            return this.BadRequest("Fichier manquant ou vide.");

        if (csvFile.Length > MaxCsvSizeBytes)
            return this.BadRequest("Fichier trop volumineux (max 5 Mo).");

        var extension = Path.GetExtension(csvFile.FileName);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return this.BadRequest("Format invalide : un fichier .csv est attendu.");

        // L'API se contente de publier le fichier ; le worker dédié le traite.
        using var stream = new MemoryStream();
        await csvFile.CopyToAsync(stream);
        await this.publisher.PublishAsync(stream.ToArray());

        return this.Ok(new { message = "Fichier reçu, en cours de traitement" });
    }
}
