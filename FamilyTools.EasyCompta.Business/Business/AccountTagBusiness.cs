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

    /// <summary>
    /// Met à jour un tag existant. On charge l'instance suivie et on copie les champs
    /// éditables, plutôt que d'attacher une 2e instance de même clé (ce que fait le
    /// <see cref="BaseBusiness{T}.Update"/> générique, qui lèverait un conflit de tracking).
    /// </summary>
    public override async Task<AccountTag> Update(AccountTag tag)
    {
        if (tag == null) return new AccountTag();

        var existing = await this._context.AccountTags.FindAsync(tag.Id);
        if (existing == null) return new AccountTag();

        existing.Name = tag.Name;
        existing.Color = tag.Color;
        existing.UpdateDate = DateTime.UtcNow;

        await this._context.SaveChangesAsync();

        return existing;
    }
}