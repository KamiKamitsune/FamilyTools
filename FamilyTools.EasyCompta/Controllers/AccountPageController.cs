using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{

    [ApiController]
    [Route("easycompta/[controller]")]
    public class AccountPageController(IAccountPageBusiness business, ILogger<AccountPageController> logger) : ControllerBase
    {

        private readonly IAccountPageBusiness business = business;
        private readonly ILogger<AccountPageController> logger = logger;

        [Route("")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var date = DateTime.Now;
                var result = await this.business.GetPageByDate(date.Month, date.Year);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]/{month}/{year}")]
        [HttpGet]
        public async Task<IActionResult> Get(int month, int year)
        {
            try
            {
                var result = await this.business.GetPageByDate(month, year);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetAllMonth()
        {
            try
            {
                return this.Ok(await this.business.GetAllMonth());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }
    }
}
