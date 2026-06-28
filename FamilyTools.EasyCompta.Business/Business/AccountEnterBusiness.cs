using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class AccountEnterBusiness(
    EasyComptaContext context,
    IAccountLineBusiness lineBusiness,
    IPaymentDoneBusiness paymentDoneBusiness,
    IUserBusiness userBusiness) : BaseBusiness<AccountEnter>(context), IAccountEnterBusiness
{
    private readonly IAccountLineBusiness _lineBusiness = lineBusiness;
    private readonly IPaymentDoneBusiness _paymentDoneBusiness = paymentDoneBusiness;
    private readonly IUserBusiness _userBusiness = userBusiness;

    public override async Task<AccountEnter> Create(AccountEnter accountEnter)
    {
        if (accountEnter == default) return new AccountEnter();

        accountEnter.Id = default;

        // Aucune ligne fournie -> répartition égale du total entre les membres
        // (même méthode que l'import CSV).
        if (accountEnter.Lines.Count == 0)
        {
            var users = await this._userBusiness.UserList();
            accountEnter.Lines = AccountLineSplitter.SplitEqually(accountEnter.TotalValue, users, _ => accountEnter.Name);
        }

        // Les entités liées (tag, type d'opération, utilisateurs) existent déjà :
        // on ne garde que leurs id pour ne pas que EF tente de les ré-insérer.
        var tagId = accountEnter.Tag?.Id ?? 0;
        var operationTypeId = accountEnter.OperationType?.Id ?? 0;
        accountEnter.Tag = null!;
        accountEnter.OperationType = null!;
        foreach (var line in accountEnter.Lines)
        {
            line.Id = default;
            line.User = null!;
        }

        CalculTotalValue(accountEnter);
        accountEnter.CreationDate = DateTime.UtcNow;

        // Une écriture appartient à la page de son mois : on la retrouve (ou on la crée).
        accountEnter.PageId = await this.ResolvePageId(accountEnter.Date);

        var entry = this._context.AccountEnters.Add(accountEnter);
        entry.Property("TagId").CurrentValue = tagId;
        entry.Property("OperationTypeId").CurrentValue = operationTypeId;

        await this._context.SaveChangesAsync();

        await this.RefreshPaymentDones(accountEnter.PageId);

        return entry.Entity;
    }

    private async Task<int> ResolvePageId(DateOnly date)
    {
        var page = await this._context.AccountPages
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(p => p.Date.Month == date.Month && p.Date.Year == date.Year);

        if (page == null)
        {
            page = new AccountPage([], new DateOnly(date.Year, date.Month, 1));
            this._context.AccountPages.Add(page);
            await this._context.SaveChangesAsync();
        }

        return page.Id;
    }

    private async Task RefreshPaymentDones(int pageId)
    {
        // Sans suivi mais avec résolution d'identité : on relit les écritures (lignes + utilisateurs
        // via AutoInclude) sans hériter des instances suivies où User a été détaché, tout en
        // partageant une même instance User par id (CalculPaymentDonesFromEnters regroupe par User).
        var enters = await this._context.AccountEnters
            .AsNoTrackingWithIdentityResolution()
            .Where(enter => enter.PageId == pageId)
            .ToListAsync();
        await this._paymentDoneBusiness.SyncFromEnters(pageId, enters);
    }

    public override async Task<AccountEnter> Update(AccountEnter accountEnter)
    {
        if (accountEnter == default) return new AccountEnter();

        var existing = await this._context.AccountEnters
            .IgnoreAutoIncludes()
            .Include(enter => enter.Lines)
            .FirstOrDefaultAsync(enter => enter.Id == accountEnter.Id);

        if (existing == null) return new AccountEnter();

        existing.Name = accountEnter.Name;
        existing.Date = accountEnter.Date;
        existing.IsDisabled = accountEnter.IsDisabled;

        var entry = this._context.Entry(existing);
        entry.Property("TagId").CurrentValue = accountEnter.Tag?.Id ?? 0;
        entry.Property("OperationTypeId").CurrentValue = accountEnter.OperationType?.Id ?? 0;

        this.SyncLines(existing, accountEnter.Lines);

        existing.TotalValue = (float)Math.Round(accountEnter.Lines.Sum(line => line.Value), 2);
        existing.UpdateDate = DateTime.UtcNow;

        await this._context.SaveChangesAsync();

        await this.RefreshPaymentDones(existing.PageId);

        return existing;
    }

    /// <summary>
    /// Réconcilie les lignes persistées avec les lignes reçues : suppression de celles
    /// qui ne sont plus présentes, mise à jour des conservées, ajout des nouvelles.
    /// </summary>
    private void SyncLines(AccountEnter existing, ICollection<AccountLine> incoming)
    {
        incoming ??= [];
        var incomingById = incoming.Where(line => line.Id > 0).ToDictionary(line => line.Id);

        foreach (var line in existing.Lines.Where(line => !incomingById.ContainsKey(line.Id)).ToList())
            this._context.AccountLines.Remove(line);

        foreach (var line in existing.Lines.Where(line => incomingById.ContainsKey(line.Id)))
        {
            var source = incomingById[line.Id];
            line.Value = source.Value;
            line.Name = existing.Name;
            line.UserId = source.UserId;
        }

        foreach (var source in incoming.Where(line => line.Id <= 0))
            existing.Lines.Add(new AccountLine
            {
                Name = existing.Name,
                Value = source.Value,
                UserId = source.UserId,
                User = null!,
            });
    }

    public override async Task<bool> Delete(int id)
    {
        var enter = await this._context.AccountEnters.FindAsync(id);
        if (enter == null) return false;

        var pageId = enter.PageId;
        this._context.AccountEnters.Remove(enter);
        await this._context.SaveChangesAsync();

        // La suppression d'une écriture change la répartition : on recalcule les règlements de la page.
        await this.RefreshPaymentDones(pageId);

        return true;
    }

    public async Task<List<AccountEnter>> CreateList(ICollection<AccountEnter> list)
    {
        List<AccountEnter> newEnters = [];

        if (list == default) return newEnters;

        foreach (var accountEnter in list)
            if (accountEnter != default
                && !await this._context.AccountEnters
                    .AnyAsync(a =>
                        a.Name == accountEnter.Name && a.TotalValue == accountEnter.TotalValue &&
                        a.Date == accountEnter.Date)
               )
            {
                accountEnter.Id = default;
                CalculTotalValue(accountEnter);
                accountEnter.CreationDate = DateTime.UtcNow;
                newEnters.Add(this._context.AccountEnters.Add(accountEnter).Entity);
            }

        await this._context.SaveChangesAsync();

        await this.GenerateLine(newEnters);

        return newEnters;
    }

    public async Task<Dictionary<int, float>> ExpensesByTagForAMonth(int month, int year)
    {
        if (month < 1 || month > 12) return [];

        return await this._context.AccountEnters
            .Where(data => data.Date.Month == month && data.Date.Year == year)
            .GroupBy(data => data.Tag.Id)
            .Select(data => new
            {
                data.Key,
                sum = data.Sum(value => value.TotalValue)
            })
            .ToDictionaryAsync(group => group.Key, group => group.sum);
    }

    public async Task<Dictionary<string, int>> TagIdByName(int defaultTagId)
    {
        // Libellés déjà tagués à la main (tag != tag par défaut), du plus ancien au plus récent.
        var tagged = await this._context.AccountEnters
            .Where(enter => EF.Property<int>(enter, "TagId") != defaultTagId)
            .OrderBy(enter => enter.Date).ThenBy(enter => enter.Id)
            .Select(enter => new { enter.Name, TagId = EF.Property<int>(enter, "TagId") })
            .ToListAsync();

        // Un même libellé a pu porter plusieurs tags au fil du temps : le plus récent l'emporte.
        return tagged
            .GroupBy(item => item.Name)
            .ToDictionary(group => group.Key, group => group.Last().TagId);
    }

    public async Task<Dictionary<int, float>> ExpensesByTagForAYear(int year)
    {
        if (year == default) return [];

        return await this._context.AccountEnters
            .Where(data => data.Date.Year == year)
            .GroupBy(data => data.Tag.Id)
            .Select(data => new
            {
                data.Key,
                sum = data.Sum(value => value.TotalValue)
            })
            .ToDictionaryAsync(group => group.Key, group => group.sum);
    }

    public async Task<Dictionary<int, float>> ExpensesByMonthForAYear(int year)
    {
        if (year == default) return [];

        return await this._context.AccountEnters
            .Where(data => data.Date.Year == year)
            .GroupBy(data => data.Date.Month)
            .Select(data => new
            {
                data.Key,
                sum = data.Sum(value => value.TotalValue)
            })
            .ToDictionaryAsync(group => group.Key, group => group.sum);
    }

    public async Task<AccountEnter> DisabledEnter(int id, bool isDisabled)
    {
        var enterUpdated = await this._context.AccountEnters.FindAsync(id);
        if (enterUpdated == null) return new AccountEnter();
        enterUpdated.IsDisabled = isDisabled;
        await this._context.SaveChangesAsync();

        await this.RefreshPaymentDones(enterUpdated.PageId);

        return enterUpdated;
    }

    internal static void CalculTotalValue(AccountEnter accountEnter)
    {
        float totalValue = (float)Math.Round(accountEnter.Lines.Sum(x => x.Value), 2);

        accountEnter.TotalValue = totalValue;
    }

    private async Task GenerateLine(List<AccountEnter> enters)
    {
        var Lines = new List<AccountLine>();
        foreach (var enter in enters)
        {
            foreach (var line in enter.Lines) line.Enter = enter;
            Lines.AddRange(enter.Lines);
        }

        await this._lineBusiness.CreateList(Lines);
    }
}
