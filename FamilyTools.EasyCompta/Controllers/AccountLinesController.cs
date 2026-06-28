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
        return this.Ok(await this.business.Create(accountLine));
    }

    [Route("[action]")]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] AccountLine accountLine)
    {
        return this.Ok(await this.business.Update(accountLine));
    }

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        return this.Ok(await this.business.Find(id));
    }

    [Route("[action]/{id}")]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        return this.Ok(await this.business.Delete(id));
    }

    [Route("[action]/{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByUserForAYear(int year)
    {
        return this.Ok(await this.business.ExpensesByUserForAYear(year));
    }

    [Route("[action]/{month}/{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByUserAndTagForAMonth(int month, int year)
    {
        return this.Ok(await this.business.ExpensesByUserAndTagForAMonth(month, year));
    }
}
