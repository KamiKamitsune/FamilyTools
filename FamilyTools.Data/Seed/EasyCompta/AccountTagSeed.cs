using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.Data.Seed.EasyCompta;

public class AccountTagSeed(EasyComptaContext EasyComptaContext) : IContextSeed
{
    public EasyComptaContext Context { get; set; } = EasyComptaContext;

    public async Task Execute()
    {
        if (this.Context.AccountTags.Any()) return;

        this.Context.AccountTags.Add(new AccountTag());
        await this.Context.SaveChangesAsync();
    }
}