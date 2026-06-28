using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.Dtos;

/// <summary>
/// DTO d'entrée pour la création/édition d'un template (la collection Enters
/// n'est pas pilotée par le client).
/// </summary>
public record TemplateDto(int? Id, string Name, DateTime Date)
{
    public Template ToEntity() => new()
    {
        Id = this.Id ?? 0,
        Name = this.Name,
        Date = this.Date,
    };
}
