using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.Dtos;

/// <summary>
/// DTO d'entrée pour une ligne d'écriture : on ne pilote que l'utilisateur (par id) et le montant.
/// </summary>
public record AccountLineDto(int? Id, int UserId, float Value);

/// <summary>
/// DTO d'entrée pour la création/édition d'une écriture (évite l'over-posting :
/// les entités liées sont référencées par id, et les champs calculés/d'audit
/// (TotalValue, CreationDate/UpdateDate) restent pilotés par le serveur).
/// </summary>
public record AccountEnterDto(
    int? Id,
    string Name,
    DateOnly Date,
    bool IsDisabled,
    int TagId,
    int OperationTypeId,
    float TotalValue,
    List<AccountLineDto>? Lines)
{
    public AccountEnter ToEntity() => new()
    {
        Id = this.Id ?? 0,
        Name = this.Name,
        Date = this.Date,
        IsDisabled = this.IsDisabled,
        TotalValue = this.TotalValue,
        // Stubs « id only » : la couche métier les rattache aux entités existantes.
        Tag = new AccountTag { Id = this.TagId },
        OperationType = new OperationType { Id = this.OperationTypeId, Name = string.Empty },
        Lines = (this.Lines ?? [])
            .Select(line => new AccountLine
            {
                Id = line.Id ?? 0,
                UserId = line.UserId,
                Value = line.Value,
                Name = this.Name,
                User = null!,
            })
            .ToList(),
    };
}
