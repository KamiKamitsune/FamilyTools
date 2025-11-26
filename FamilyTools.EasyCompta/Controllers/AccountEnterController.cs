using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("easycompta/[controller]")]
    public class AccountEnterController(IAccountEnterBusiness business, ILogger<AccountEnterController> logger) : ControllerBase
    {
        private readonly ILogger<AccountEnterController> logger = logger;
        private readonly IAccountEnterBusiness business = business;

        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                return this.Ok(await this.business.Find(id));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] AccountEnter accountEnter)
        {
            try
            {
                return this.Ok(await this.business.Create(accountEnter));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]")]
        [HttpPost]
        [HttpPut]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] AccountEnter accountEnter)
        {
            try
            {
                return this.Ok(await this.business.Update(accountEnter));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]/{id}")]
        [HttpGet]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return this.Ok(await this.business.Delete(id));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]/{month}/{year}")]
        [HttpGet]
        public async Task<IActionResult> ExpensesByTagForAMonth(int month, int year)
        {
            try
            {
                return this.Ok(await this.business.ExpensesByTagForAMonth(month, year));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]/{id}")]
        [HttpPatch]
        public async Task<IActionResult> Desabled(int id, [FromBody]bool desabled)
        {
            try
            {
                var result = await this.business.DesabledEnter(id, desabled);
                if (result == null)
                {
                    return this.NotFound();
                }
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest(ex);
            }
        }
    }
}
