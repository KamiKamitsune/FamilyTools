using FamilyTools.EasyCompta.Dtos;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class AccountTagController(IAccountTagBusiness business, ILogger<AccountTagController> logger) : ControllerBase
{
    private readonly IAccountTagBusiness business = business;
    private readonly ILogger<AccountTagController> logger = logger;


    [Route("[action]")]
    [Route("")]
    [HttpGet]
    public async Task<ActionResult> List()
    {
        var tags = await this.business.TagList();

        return this.Ok(tags);
    }

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        return this.Ok(await this.business.Find(id));
    }

    [Route("[action]")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] AccountTagDto dto)
    {
        return this.Ok(await this.business.Create(dto.ToEntity()));
    }

    [Route("[action]")]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] AccountTagDto dto)
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
