using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.Data.Seed.EasyCompta;

public class AccountTagSeed(EasyComptaContext EasyComptaContext) : IContextSeed
{
    public EasyComptaContext Context { get; set; } = EasyComptaContext;

    /// <summary>Catégories de dépenses de la vie courante (hors sentinelle "non classé").</summary>
    private static readonly (string Name, string Color)[] ExpenseTags =
    [
        ("Alimentation", "#4CAF50"),
        ("Logement", "#795548"),
        ("Transport", "#2196F3"),
        ("Restauration", "#FF9800"),
        ("Santé", "#E91E63"),
        ("Loisirs", "#9C27B0"),
        ("Énergie & factures", "#FFC107"),
        ("Habillement", "#00BCD4"),
        ("Abonnements", "#607D8B"),
        ("Épargne", "#3F51B5"),
        ("Divers", "#9E9E9E"),
    ];

    public async Task Execute()
    {
        // Sentinelle "non classé" : doit exister et rester le 1er tag, car l'import CSV et
        // l'auto-tag s'appuient dessus (cf. AccountTagBusiness.DefaultTag). On ne la (re)crée
        // que si la table est vide, pour ne pas bousculer une base déjà initialisée.
        if (!await this.Context.AccountTags.AnyAsync())
        {
            this.Context.AccountTags.Add(new AccountTag { Name = "Non classé", Color = "#BDBDBD" });
            await this.Context.SaveChangesAsync();
        }

        // Idempotent : on n'ajoute que les catégories manquantes (par nom), sans toucher
        // aux tags existants — utile quand on enrichit la liste sur une base déjà peuplée.
        var existingNames = await this.Context.AccountTags.Select(tag => tag.Name).ToListAsync();

        var missing = ExpenseTags
            .Where(tag => !existingNames.Contains(tag.Name))
            .Select(tag => new AccountTag { Name = tag.Name, Color = tag.Color })
            .ToList();

        if (missing.Count > 0)
        {
            this.Context.AccountTags.AddRange(missing);
            await this.Context.SaveChangesAsync();
        }
    }
}
