using FamilyTools.Data.Models.EasyCompta;
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
    public async Task<IActionResult> Create([FromBody] Template template)
    {
        try
        {
            return this.Ok(await this.business.Create(template));
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
    public async Task<IActionResult> Edit([FromBody] Template template)
    {
        try
        {
            return this.Ok(await this.business.Update(template));
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