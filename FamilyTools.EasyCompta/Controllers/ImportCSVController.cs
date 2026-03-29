using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class ImportCSVController(IImportCSVBusiness business, ILogger<ImportCSVController> logger) : ControllerBase
{
    private readonly IImportCSVBusiness business = business;
    private readonly ILogger<ImportCSVController> logger = logger;

    [Route("[action]")]
    [Route("")]
    [HttpPost]
    public async Task<ActionResult> ImportCSV(IFormFile csvFile)
    {
        try
        {
            var result = await this.business.ImportCSVFile(csvFile);

            return result
                ? this.Ok(new { message = "Fichier reçu, en cour de traitement" })
                : this.BadRequest("Fichier invalide");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex.Message);
            return this.BadRequest(ex.Message);
        }
    }
}