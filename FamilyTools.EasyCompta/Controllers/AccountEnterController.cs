using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.Dtos;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]/[action]/")]
public class AccountEnterController(IAccountEnterBusiness business, ILogger<AccountEnterController> logger)
    : ControllerBase
{
    private readonly IAccountEnterBusiness _business = business;
    private readonly ILogger<AccountEnterController> _logger = logger;

    [Route("{id}")]
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        return this.Ok(await this._business.Find(id));
    }

    [Route("")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] AccountEnterDto dto)
    {
        return this.Ok(await this._business.Create(dto.ToEntity()));
    }

    [Route("")]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] AccountEnterDto dto)
    {
        return this.Ok(await this._business.Update(dto.ToEntity()));
    }

    [Route("{id}")]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        return this.Ok(await this._business.Delete(id));
    }

    [Route("{month}/{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByTagForAMonth(int month, int year)
    {
        return this.Ok(await this._business.ExpensesByTagForAMonth(month, year));
    }

    [Route("{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByTagForAYear(int year)
    {
        return this.Ok(await this._business.ExpensesByTagForAYear(year));
    }

    [Route("{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByMonthForAYear(int year)
    {
        return this.Ok(await this._business.ExpensesByMonthForAYear(year));
    }

    [Route("{id}")]
    [HttpPatch]
    public async Task<IActionResult> Disabled(int id, [FromBody] bool disabled)
    {
        var result = await this._business.DisabledEnter(id, disabled);
        if (result == null) return this.NotFound();
        return this.Ok(result);
    }
}
