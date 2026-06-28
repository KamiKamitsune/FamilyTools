using System.Xml.Linq;

using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.Data.Seed.EasyCompta;

public class OperationTypeSeed( EasyComptaContext EasyComptaContext) : IContextSeed
{
    public EasyComptaContext Context { get; set; } = EasyComptaContext;

    public async Task Execute()
    {
        if (this.Context.AccountTags.Any()) return;

        var operationTypesList = new List<string>()
        {
            "Unknown",
            "Prelevement",
            "PaiementCarte",
            "VirementRecu",
            "VirementEmis",
            "RemboursementPret",
            "Reglement",
            "Avoir",
            "Cotisation",
            "ChequeEmis"

        };

        foreach (var operationType in operationTypesList)
        {
            this.Context.OperationTypes.Add(new OperationType()
            {
                Name = operationType
            });
        }

        await this.Context.SaveChangesAsync();
    }
}
