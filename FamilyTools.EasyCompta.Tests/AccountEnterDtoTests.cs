using FamilyTools.EasyCompta.Dtos;

namespace FamilyTools.EasyCompta.Tests;

public class AccountEnterDtoTests
{
    [Fact]
    public void ToEntity_MapsIdsToStubs_AndCopiesNameToLines()
    {
        var dto = new AccountEnterDto(
            Id: 5,
            Name: "Courses",
            Date: new DateOnly(2026, 6, 8),
            IsDisabled: true,
            TagId: 3,
            OperationTypeId: 7,
            TotalValue: 30.75f,
            Lines: [new AccountLineDto(11, 2, 10.5f), new AccountLineDto(null, 4, 20.25f)]);

        var entity = dto.ToEntity();

        Assert.Equal(5, entity.Id);
        Assert.Equal("Courses", entity.Name);
        Assert.Equal(new DateOnly(2026, 6, 8), entity.Date);
        Assert.True(entity.IsDisabled);
        Assert.Equal(30.75f, entity.TotalValue);
        // Les entités liées ne sont référencées que par leur id (stubs).
        Assert.Equal(3, entity.Tag.Id);
        Assert.Equal(7, entity.OperationType.Id);

        Assert.Equal(2, entity.Lines.Count);
        var first = entity.Lines.First();
        Assert.Equal(11, first.Id);
        Assert.Equal(2, first.UserId);
        Assert.Equal(10.5f, first.Value);
        Assert.Equal("Courses", first.Name); // le nom de ligne reprend celui de l'écriture

        var second = entity.Lines.Last();
        Assert.Equal(0, second.Id); // Id null -> 0 (nouvelle ligne)
        Assert.Equal(4, second.UserId);
    }

    [Fact]
    public void ToEntity_NullId_AndNullLines_AreHandled()
    {
        var dto = new AccountEnterDto(null, "X", new DateOnly(2026, 1, 1), false, 1, 1, 0f, null);

        var entity = dto.ToEntity();

        Assert.Equal(0, entity.Id);
        Assert.Empty(entity.Lines);
    }
}
