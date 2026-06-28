using FamilyTools.EasyCompta.Dtos;

namespace FamilyTools.EasyCompta.Tests;

/// <summary>
/// Mapping ToEntity des DTOs d'entrée : on vérifie surtout que l'Id null (création)
/// devient 0 et que les champs pilotés par le client sont bien recopiés.
/// </summary>
public class DtoMappingTests
{
    [Fact]
    public void AccountTagDto_ToEntity_CopiesFields()
    {
        var entity = new AccountTagDto(Id: 4, Name: "Courses", Color: "#FF0000").ToEntity();

        Assert.Equal(4, entity.Id);
        Assert.Equal("Courses", entity.Name);
        Assert.Equal("#FF0000", entity.Color);
    }

    [Fact]
    public void AccountTagDto_ToEntity_NullId_BecomesZero()
    {
        var entity = new AccountTagDto(Id: null, Name: "Loisirs", Color: "#00FF00").ToEntity();

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void UserDto_ToEntity_CopiesFields()
    {
        var entity = new UserDto(Id: 2, FirstName: "Jean", LastName: "Dupont", UserName: "jdupont").ToEntity();

        Assert.Equal(2, entity.Id);
        Assert.Equal("Jean", entity.FirstName);
        Assert.Equal("Dupont", entity.LastName);
        Assert.Equal("jdupont", entity.UserName);
    }

    [Fact]
    public void UserDto_ToEntity_NullId_BecomesZero()
    {
        var entity = new UserDto(Id: null, FirstName: "A", LastName: "B", UserName: "ab").ToEntity();

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void TemplateDto_ToEntity_CopiesFields()
    {
        var date = new DateTime(2026, 6, 12);
        var entity = new TemplateDto(Id: 9, Name: "Mois type", Date: date).ToEntity();

        Assert.Equal(9, entity.Id);
        Assert.Equal("Mois type", entity.Name);
        Assert.Equal(date, entity.Date);
    }

    [Fact]
    public void TemplateDto_ToEntity_NullId_BecomesZero()
    {
        var entity = new TemplateDto(Id: null, Name: "X", Date: default).ToEntity();

        Assert.Equal(0, entity.Id);
    }
}
