using FamilyTools.Data.Models.EasyCompta;
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
        try
        {
            return this.Ok(await this._business.Find(id));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("")]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] AccountEnter accountEnter)
    {
        try
        {
            return this.Ok(await this._business.Create(accountEnter));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("")]
    [HttpPost]
    [HttpPut]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] AccountEnter accountEnter)
    {
        try
        {
            return this.Ok(await this._business.Update(accountEnter));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("{id}")]
    [HttpGet]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            return this.Ok(await this._business.Delete(id));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("{month}/{year}")]
    [HttpGet]
    public async Task<IActionResult> ExpensesByTagForAMonth(int month, int year)
    {
        try
        {
            return this.Ok(await this._business.ExpensesByTagForAMonth(month, year));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest();
        }
    }

    [Route("{id}")]
    [HttpPatch]
    public async Task<IActionResult> Desabled(int id, [FromBody] bool desabled)
    {
        try
        {
            var result = await this._business.DesabledEnter(id, desabled);
            if (result == null) return this.NotFound();
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message);
            return this.BadRequest(ex);
        }
    }
}