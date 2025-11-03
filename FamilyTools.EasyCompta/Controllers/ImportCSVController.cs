using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("easycompta/[controller]")]
    public class ImportCSVController(IImportCSVBusiness business, ILogger<ImportCSVController> logger) : Controller
    {
        private readonly ILogger<ImportCSVController> logger = logger;
        private readonly IImportCSVBusiness business = business;

        [Route("[action]")]
        [Route("")]
        [HttpPost]
        public async Task<ActionResult> ImportCSV(IFormFile csvFile)
        {
            try
            {
                var result = this.business.ImportCSVFile(csvFile);

                return this.Ok(new { message = $"Fichier reçu et {result}" });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest(ex.Message);
            }
        }
    }
}