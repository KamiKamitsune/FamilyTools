using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("easycompta/[controller]")]
    public class OperationTypeController(IOperationTypeBusiness business, ILogger<OperationTypeController> logger) : ControllerBase
    {

        private readonly IOperationTypeBusiness business = business;
        private readonly ILogger<OperationTypeController> logger = logger;

        [Route("[action]")]
        [Route("")]
        [HttpGet]
        [OutputCache(Duration = 120)]
        public async Task<ActionResult> List()
        {
            var OperationTypes = await this.business.OperationTypeList();

            return this.Ok(OperationTypes);
        }
    }
}
