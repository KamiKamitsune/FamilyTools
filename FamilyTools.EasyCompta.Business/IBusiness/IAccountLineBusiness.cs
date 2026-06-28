using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness;

public interface IAccountLineBusiness : IBaseBusiness<AccountLine>
{
    Task CreateList(List<AccountLine> lines);
    Task<Dictionary<int, float>> ExpensesByUserForAYear(int year);

    /// <summary>
    /// Montant dépensé par membre et par catégorie (tag) pour un mois — « qui dépense quoi ».
    /// </summary>
    Task<List<UserTagExpense>> ExpensesByUserAndTagForAMonth(int month, int year);
}

/// <summary>Montant dépensé par un membre sur une catégorie (tag) donnée.</summary>
public record UserTagExpense(int UserId, int TagId, float Amount);