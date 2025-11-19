using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace FamilyTools.EasyCompta.Business
{
    public class PaymentDoneBusiness(EasyComptaContext context) : BaseBusiness<PaymentDone>(context), IPaymentDoneBusiness
    {
        public async Task<List<PaymentDone>> CreateListFromPage(AccountPage page)
        {
            var newPaymentDones = this.getPaymentDonesFromPages(page);

            this._context.PaymentDones.AddRange(this.getPaymentDonesFromPages(page));

            await _context.SaveChangesAsync();
            return newPaymentDones;
        }

        public async Task<List<PaymentDone>> UpdateListFromPage(AccountPage page)
        {
            var newpayementDones = this.getPaymentDonesFromPages(page);
            var updatePaymentDones = page.PaymentDones.Join(newpayementDones,
                existing => existing.User.Id,
                newly => newly.User.Id,
                (existing, newly) =>
                {
                    existing.Total = newly.Total;
                    return existing;
                });
            this._context.PaymentDones.UpdateRange(updatePaymentDones);
            await _context.SaveChangesAsync();

            return updatePaymentDones.ToList();

        }

        public List<PaymentDone> getPaymentDonesFromPages(AccountPage page)
        {
            if (page.Enters?.Count > 0)
            {
                page.Total = page.Enters.Sum(enter => enter.TotalValue);

                return page.Enters
                    .SelectMany(enter => enter.Lines)
                    .GroupBy(line => line.User)
                    .Select(groupe => new PaymentDone()
                    {
                        PaymentIsDone = false,
                        User = groupe.Key,
                        Total = groupe.Sum(x => x.Value)
                    }).
                    ToList();
            }
            return [];
        }      
    }
}
