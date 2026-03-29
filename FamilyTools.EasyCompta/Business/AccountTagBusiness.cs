using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class AccountTagBusiness(EasyComptaContext context) : BaseBusiness<AccountTag>(context), IAccountTagBusiness
{
    public async Task<List<AccountTag>> TagList()
    {
        return await this._context.AccountTags.ToListAsync() ?? [];
    }

    public async Task<AccountTag> DefaultTag()
    {
        return await this._context.AccountTags.FirstOrDefaultAsync();
    }
}