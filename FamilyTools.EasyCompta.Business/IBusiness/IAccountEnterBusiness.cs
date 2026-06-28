using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness;

public interface IAccountEnterBusiness : IBaseBusiness<AccountEnter>
{
    Task<List<AccountEnter>> CreateList(ICollection<AccountEnter> list);
    Task<AccountEnter> DisabledEnter(int id, bool isDisabled);
    Task<Dictionary<int, float>> ExpensesByTagForAMonth(int month, int year);

    /// <summary>Total des écritures par catégorie (tag) pour une année donnée.</summary>
    Task<Dictionary<int, float>> ExpensesByTagForAYear(int year);

    /// <summary>Total des écritures par mois (1-12) pour une année — évolution annuelle.</summary>
    Task<Dictionary<int, float>> ExpensesByMonthForAYear(int year);

    /// <summary>
    /// Pour chaque libellé déjà tagué manuellement (tag différent du tag par défaut),
    /// renvoie le dernier tag attribué. Sert à appliquer le même tag aux prochaines
    /// écritures homonymes (auto-tag à l'import).
    /// </summary>
    Task<Dictionary<string, int>> TagIdByName(int defaultTagId);
}