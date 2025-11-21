using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(IUserBusiness business, ILogger<UserController> logger) : ControllerBase
    {

        private readonly ILogger<UserController> logger = logger;
        private readonly IUserBusiness business = business;

        [Route("[action]")]
        [Route("")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try
            {
                var users = await this.business.UserList();

                return this.Ok(users);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest();
            }
        }

        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<ActionResult> Index(int id)
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
        public async Task<IActionResult> Create([FromBody] User user)
        {
            try
            {
                return this.Ok(await this.business.Create(user));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return this.BadRequest(ex.Message);
            }
        }

        [Route("[action]")]
        [HttpPost]
        [HttpPut]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] User user)
        {
            try
            {
                return this.Ok(await this.business.Update(user));
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
