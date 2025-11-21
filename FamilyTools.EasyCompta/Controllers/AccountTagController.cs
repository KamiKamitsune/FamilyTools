using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("easycompta/[controller]")]
    public class AccountTagController(IAccountTagBusiness business, ILogger<AccountTagController> logger) : ControllerBase
    {
        private readonly ILogger<AccountTagController> logger = logger;
        private readonly IAccountTagBusiness business = business;


        [Route("[action]")]
        [Route("")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try
            {
                var tags = await this.business.TagList();

                return this.Ok(tags);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

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
        public async Task<IActionResult> Create([FromBody] AccountTag accounTag)
        {
            try
            {
                return this.Ok(await this.business.Create(accounTag));
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
        public async Task<IActionResult> Edit([FromBody] AccountTag accounTag)
        {
            try
            {
                return this.Ok(await this.business.Update(accounTag));
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
    }
}
