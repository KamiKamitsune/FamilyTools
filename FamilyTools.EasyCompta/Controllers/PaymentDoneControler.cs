using FamilyTools.EasyCompta.IBusiness;

using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta.Controllers
{
    [ApiController]
    [Route("easycompta/[controller]")]
    public class PaymentDoneController(IPaymentDoneBusiness paymentDoneBusiness) : ControllerBase
    {
        
        private readonly IPaymentDoneBusiness _paymentDoneBusiness = paymentDoneBusiness;

        // easycompta/PaymentDone/5/true
        [Route("[action]/{id}")]
        [HttpPatch]
        public async Task<IActionResult> Patch(int id,[FromBody] bool status)
        {
            try
            {
                var paymentDone = await this._paymentDoneBusiness.UpdateStatePaymentDone(id, status);
                return this.Ok(paymentDone);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }            
        }
    }
}
