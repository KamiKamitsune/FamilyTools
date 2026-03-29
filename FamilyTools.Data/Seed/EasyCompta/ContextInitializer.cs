using FamilyTools.Data.Context;

namespace FamilyTools.Data.Seed.EasyCompta;

public class ContextInitializer
{
    public async Task Seed(EasyComptaContext context)
    {
        List<IContextSeed> listSeed =
        [
            new AccountTagSeed(context),
            new UserSeed(context)
        ];

        foreach (var contextSeed in listSeed) await contextSeed.Execute();
    }
}