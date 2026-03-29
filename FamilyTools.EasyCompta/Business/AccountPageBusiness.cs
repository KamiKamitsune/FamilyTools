using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace FamilyTools.EasyCompta.Business;

public class AccountPageBusiness(
    EasyComptaContext context,
    IAccountEnterBusiness accountEnterBusiness,
    IPaymentDoneBusiness paymentDoneBusiness) : BaseBusiness<AccountPage>(context), IAccountPageBusiness
{
    private readonly IAccountEnterBusiness _accountEnterBusiness = accountEnterBusiness;
    private readonly IPaymentDoneBusiness _paymentDoneBusiness = paymentDoneBusiness;

    public async Task<AccountPage> GetPageByDate(int month, int year)
    {
        if (await this._context.AccountPages.AnyAsync() && month != default && year != default)
        {
            var page = await this._context.AccountPages
                           .Where(page => page.Date.Month == month && page.Date.Year == year)
                           .FirstOrDefaultAsync()
                       ?? new AccountPage();

            page.PaymentDones = await this._paymentDoneBusiness.UpdateListFromPage(page);

            return page;
        }

        return new AccountPage();
    }

    public async Task CreateOrUpdateListPage(List<AccountPage> pages)
    {
        foreach (var page in pages)
        {
            var pageExiste = await this._context.AccountPages
                .Where(x => x.Date.Month == page.Date.Month && x.Date.Year == page.Date.Year).FirstOrDefaultAsync();
            if (pageExiste == default)
            {
                page.PaymentDones = this._paymentDoneBusiness.CalculPaymentDonesFromEnters(page.Enters);
                this._context.AccountPages.Add(page);
            }
            else
            {
                var enterAdd = await this._accountEnterBusiness.CreateList(page.Enters);
                if (enterAdd.Count != 0)
                {
                    page.PaymentDones = await this._paymentDoneBusiness.CreateListFromPage(page);
                    pageExiste.Enters.AddRange(enterAdd);
                }
            }

            await this._context.SaveChangesAsync();
        }
    }

    public async Task<List<DateOnly>> GetAllMonth()
    {
        return await this._context.AccountPages.Select(x => x.Date).ToListAsync();
    }
}