using FamilyTools.EasyCompta.IBusiness;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers;

[ApiController]
[Route("easycompta/[controller]")]
public class PaymentDoneController(IPaymentDoneBusiness paymentDoneBusiness, ILogger<PaymentDoneController> logger)
    : ControllerBase
{
    private readonly IPaymentDoneBusiness _business = paymentDoneBusiness;
    private readonly ILogger<PaymentDoneController> logger = logger;

    // easycompta/PaymentDone/5/true
    [Route("[action]/{id}")]
    [HttpPatch]
    public async Task<IActionResult> Patch(int id, [FromBody] bool status)
    {
        var paymentDone = await this._business.UpdateStatePaymentDone(id, status);
        return this.Ok(paymentDone);
    }

    [Route("[action]/{pageId}")]
    [HttpGet]
    public async Task<IActionResult> GetByPageId(int pageId)
    {
        var result = await this._business.GetListByPageId(pageId);
        return this.Ok(result);
    }
}
