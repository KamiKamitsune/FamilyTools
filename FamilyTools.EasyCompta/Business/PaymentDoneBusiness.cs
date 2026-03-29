using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class PaymentDoneBusiness(EasyComptaContext context) : BaseBusiness<PaymentDone>(context), IPaymentDoneBusiness
{
    public async Task<List<PaymentDone>> CreateListFromPage(AccountPage page)
    {
        var newPaymentDones = this.CalculPaymentDonesFromEnters(page.Enters);

        this._context.PaymentDones.AddRange(newPaymentDones);

        await this._context.SaveChangesAsync();
        return newPaymentDones;
    }

    public async Task<List<PaymentDone>> UpdateListFromPage(AccountPage page)
    {
        var newpayementDones = this.CalculPaymentDonesFromEnters(page.Enters);
        var updatePaymentDones = page.PaymentDones.Join(newpayementDones,
            existing => existing.User.Id,
            newly => newly.User.Id,
            (existing, newly) =>
            {
                existing.Total = newly.Total;
                return existing;
            });
        this._context.PaymentDones.UpdateRange(updatePaymentDones);
        await this._context.SaveChangesAsync();

        return updatePaymentDones.ToList();
    }

    public async Task<List<PaymentDone>> UpdateListFromPage(List<AccountEnter> enters)
    {
        var newPaymentDones = this.CalculPaymentDonesFromEnters(enters);

        var newPaymentsDict = newPaymentDones.ToDictionary(p => (p.UserId, p.PageId));

        var userIds = newPaymentDones.Select(p => p.UserId).ToList();
        var pageIds = newPaymentDones.Select(p => p.PageId).ToList();

        var existingPayments = await this._context.PaymentDones
            .Where(p => userIds.Contains(p.UserId) && pageIds.Contains(p.PageId))
            .ToListAsync();

        foreach (var existing in existingPayments)
            if (newPaymentsDict.TryGetValue((existing.UserId, existing.PageId), out var newPayment))
                existing.Total = newPayment.Total;

        await this._context.SaveChangesAsync();

        return existingPayments;
    }

    public List<PaymentDone> CalculPaymentDonesFromEnters(IEnumerable<AccountEnter> enters)
    {
        if (enters.Any())
        {
            var payments = new List<PaymentDone>();
            var enterByPage = enters.Where(e => !e.IsDisabled).GroupBy(enter => enter.PageId).ToList();
            foreach (var pageGroup in enterByPage)
            {
                var paymentByPage = pageGroup
                    .SelectMany(enter => enter.Lines)
                    .GroupBy(line => line.User)
                    .Select(groupe => new PaymentDone
                    {
                        PageId = pageGroup.Key,
                        PaymentIsDone = false,
                        User = groupe.Key,
                        UserId = groupe.Key.Id,
                        Total = groupe.Sum(x => x.Value)
                    })
                    .ToList();
                payments.AddRange(paymentByPage);
            }

            return payments;
        }

        return [];
    }

    public async Task<PaymentDone> UpdateStatePaymentDone(int id, bool status)
    {
        if (id == default) throw new ArgumentException("L'identifiant du paiement est invalide.");

        var payment = this._context.PaymentDones.Where(x => x.Id == id).FirstOrDefault();

        if (payment == default) throw new ArgumentException("Le paiement n'existe pas.");

        payment.PaymentIsDone = status;
        this._context.Update(payment);
        await this._context.SaveChangesAsync();

        return payment;
    }

    public async Task<List<PaymentDone>> GetListByPageId(int pageId)
    {
        if (pageId != default) return await this._context.PaymentDones.Where(x => x.PageId == pageId).ToListAsync();
        return [];
    }
}