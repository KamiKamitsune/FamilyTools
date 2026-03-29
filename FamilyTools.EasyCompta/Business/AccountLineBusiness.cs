using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class AccountLineBusiness(EasyComptaContext context) : BaseBusiness<AccountLine>(context), IAccountLineBusiness
{
    public async Task CreateList(List<AccountLine> lines)
    {
        if (lines?.Count >= 0)
        {
            foreach (var line in lines)
                if (await this.ControlDuplicate(line))
                    this._context.AccountLines.Add(line);

            await this._context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<int, int>> ExpensesByUserForAYear(int year)
    {
        if (year != default)
            await this._context.AccountLines.Where(data => data.Enter.Date.Year == year)
                .GroupBy(data => data.UserId)
                .Select(group => new
                {
                    group.Key,
                    sum = group.Sum(value => value.Value)
                })
                .ToDictionaryAsync(group => group.Key, group => group.sum);

        return [];
    }

    private async Task<bool> ControlDuplicate(AccountLine line)
    {
        return !await this._context.AccountLines.AnyAsync(x =>
            x.Name == line.Name && x.Value == line.Value && x.User == x.User);
    }
}