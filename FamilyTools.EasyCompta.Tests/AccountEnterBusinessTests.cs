using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.Business;

namespace FamilyTools.EasyCompta.Tests;

public class AccountEnterBusinessTests
{
    [Fact]
    public void CalculTotalValue_SumsAllLines()
    {
        // Régression du bug `totalValue = +line.Value` qui ne gardait que la dernière ligne
        var enter = new AccountEnter
        {
            Lines = new List<AccountLine>
            {
                new() { Name = "L1", User = new User(), Value = 10.50f },
                new() { Name = "L2", User = new User(), Value = 20.25f }
            }
        };

        AccountEnterBusiness.CalculTotalValue(enter);

        Assert.Equal(30.75, (double)enter.TotalValue, 2);
    }

    [Fact]
    public void CalculTotalValue_NoLines_IsZero()
    {
        var enter = new AccountEnter();

        AccountEnterBusiness.CalculTotalValue(enter);

        Assert.Equal(0.0, (double)enter.TotalValue, 2);
    }
}
