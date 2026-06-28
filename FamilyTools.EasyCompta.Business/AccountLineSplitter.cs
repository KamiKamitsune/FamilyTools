using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.Business;

/// <summary>
/// Répartition d'un montant entre les membres du foyer. Méthode unique partagée par
/// l'import CSV et la création manuelle d'écriture.
/// </summary>
public static class AccountLineSplitter
{
    /// <summary>
    /// Crée une ligne par utilisateur en répartissant <paramref name="total"/> à parts égales,
    /// arrondies au centime. Le reliquat d'arrondi est porté par les premières lignes pour que
    /// la somme des lignes soit exactement égale au total.
    /// </summary>
    public static List<AccountLine> SplitEqually(float total, IReadOnlyList<User> users, Func<User, string> nameSelector)
    {
        var lines = new List<AccountLine>(users.Count);
        if (users.Count == 0) return lines;

        // On raisonne en centimes pour un arrondi exact et un reliquat maîtrisé.
        var totalCents = (int)Math.Round((decimal)total * 100m, MidpointRounding.AwayFromZero);
        var baseCents = totalCents / users.Count;
        var remainder = totalCents - baseCents * users.Count;
        var sign = Math.Sign(remainder);
        var extra = Math.Abs(remainder);

        for (var i = 0; i < users.Count; i++)
        {
            var cents = baseCents + (i < extra ? sign : 0);
            lines.Add(new AccountLine
            {
                Name = nameSelector(users[i]),
                User = users[i],
                UserId = users[i].Id,
                Value = cents / 100f,
            });
        }

        return lines;
    }
}
