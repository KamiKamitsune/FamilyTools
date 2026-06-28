using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.Dtos;

/// <summary>
/// DTO d'entrée pour la création/édition d'un membre (évite l'over-posting :
/// les champs d'audit Id/CreationDate/UpdateDate ne sont pas pilotés par le client).
/// </summary>
public record UserDto(int? Id, string FirstName, string LastName, string UserName)
{
    public User ToEntity() => new()
    {
        Id = this.Id ?? 0,
        FirstName = this.FirstName,
        LastName = this.LastName,
        UserName = this.UserName,
    };
}
