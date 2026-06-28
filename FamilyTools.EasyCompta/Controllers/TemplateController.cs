using FamilyTools.EasyCompta.Dtos;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class TemplateController(ITemplateBusiness business, ILogger<TemplateController> logger) : ControllerBase
{
    private readonly ITemplateBusiness business = business;
    private readonly ILogger<TemplateController> logger = logger;

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        return this.Ok(await this.business.Find(id));
    }

    [Route("[action]")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] TemplateDto dto)
    {
        return this.Ok(await this.business.Create(dto.ToEntity()));
    }

    [Route("[action]")]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] TemplateDto dto)
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
