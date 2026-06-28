using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.Dtos;

/// <summary>
/// DTO d'entrée pour la création/édition d'un tag (évite l'over-posting).
/// </summary>
public record AccountTagDto(int? Id, string Name, string Color)
{
    public AccountTag ToEntity() => new()
    {
        Id = this.Id ?? 0,
        Name = this.Name,
        Color = this.Color,
    };
}
