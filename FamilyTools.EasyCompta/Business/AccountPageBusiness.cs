using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

using Microsoft.EntityFrameworkCore;

using NuGet.Packaging;

namespace FamilyTools.EasyCompta.Business
{
    public class AccountPageBusiness(EasyComptaContext context, IAccountEnterBusiness accountEnterBusiness) : BaseBusiness<AccountPage>(context), IAccountPageBusiness
    {
        private readonly IAccountEnterBusiness _accountEnterBusiness = accountEnterBusiness;

        public async Task<AccountPage> GetPageByDate(int month, int year) {
            if (await _context.AccountPages.AnyAsync() && month != default && year != default)
            {
                return await this._context.AccountPages.Where(page => page.Date.Month == month && page.Date.Year == year).
                    Include(page => page.Enters).FirstOrDefaultAsync() ?? new AccountPage();
            }
            return new AccountPage();
        }

        public async Task CreateListPage(List<AccountPage> pages)
        {
            foreach (var page in pages)
            {
                var pageExiste = await this._context.AccountPages.Where(x => x.Date.Month == page.Date.Month && x.Date.Year == page.Date.Year).FirstOrDefaultAsync();
                if (pageExiste == default)
                {
                    this._context.AccountPages.Add(page);
                }
                else
                {
                    var enterAdd = await this._accountEnterBusiness.CreateList(page.Enters);
                    if (enterAdd.Count != 0)
                    {
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
}
