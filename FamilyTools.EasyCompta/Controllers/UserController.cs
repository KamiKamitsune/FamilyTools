using FamilyTools.EasyCompta.Dtos;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserBusiness business, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserBusiness business = business;

    private readonly ILogger<UserController> logger = logger;

    [Route("[action]")]
    [Route("")]
    [HttpGet]
    public async Task<ActionResult> List()
    {
        var users = await this.business.UserList();

        return this.Ok(users);
    }

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<ActionResult> Index(int id)
    {
        return this.Ok(await this.business.Find(id));
    }

    [Route("[action]")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] UserDto dto)
    {
        return this.Ok(await this.business.Create(dto.ToEntity()));
    }

    [Route("[action]")]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] UserDto dto)
    {
        return this.Ok(await this.business.Update(dto.ToEntity()));
    }

    [Route("[action]/{id}")]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        return this.Ok(await this.business.Delete(id));
    }
}
