using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class AccountLinesController(IAccountLineBusiness business, ILogger<AccountLinesController> logger)
    : ControllerBase
{
    private readonly IAccountLineBusiness business = business;
    private readonly ILogger<AccountLinesController> logger = logger;

    [Route("[action]")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] AccountLine accountLine)
    {
        try
        {
            return this.Ok(await this.business.Create(accountLine));
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
    public async Task<IActionResult> Edit([FromBody] AccountLine accountLine)
    {
        try
        {
            return this.Ok(await this.business.Update(accountLine));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<IActionResult> Get(int id)
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

    [Route("[action]/{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByUserForAYear(int year)
    {
        try
        {
            return this.Ok(await this.business.ExpensesByUserForAYear(year));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }
}