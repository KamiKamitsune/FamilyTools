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

    public async Task<Dictionary<int, float>> ExpensesByUserForAYear(int year)
    {
        if (year == default) return [];

        return await this._context.AccountLines
            .Where(data => data.Enter.Date.Year == year)
            .GroupBy(data => data.UserId)
            .Select(group => new
            {
                group.Key,
                sum = group.Sum(value => value.Value)
            })
            .ToDictionaryAsync(group => group.Key, group => group.sum);
    }

    public async Task<List<UserTagExpense>> ExpensesByUserAndTagForAMonth(int month, int year)
    {
        if (month < 1 || month > 12) return [];

        var rows = await this._context.AccountLines
            .Where(line => line.Enter.Date.Month == month && line.Enter.Date.Year == year)
            .GroupBy(line => new { line.UserId, TagId = line.Enter.Tag.Id })
            .Select(group => new
            {
                group.Key.UserId,
                group.Key.TagId,
                amount = group.Sum(value => value.Value)
            })
            .ToListAsync();

        return [.. rows.Select(row => new UserTagExpense(row.UserId, row.TagId, row.amount))];
    }

    private async Task<bool> ControlDuplicate(AccountLine line)
    {
        return !await this._context.AccountLines.AnyAsync(x =>
            x.Name == line.Name && x.Value == line.Value && x.User == x.User);
    }
}