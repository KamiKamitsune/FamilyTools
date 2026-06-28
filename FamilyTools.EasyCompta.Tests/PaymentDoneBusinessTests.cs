using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.Business;

namespace FamilyTools.EasyCompta.Tests;

public class PaymentDoneBusinessTests
{
    [Fact]
    public void CalculPaymentDonesFromEnters_SumsPerUser_AndIgnoresDisabledEnters()
    {
        var alice = new User { Id = 1, UserName = "Alice" };
        var bob = new User { Id = 2, UserName = "Bob" };

        var enters = new List<AccountEnter>
        {
            new()
            {
                PageId = 10,
                IsDisabled = false,
                Lines =
                [
                    new AccountLine { Name = "a", User = alice, UserId = 1, Value = 30 },
                    new AccountLine { Name = "a", User = bob, UserId = 2, Value = 20 }
                ]
            },
            new()
            {
                PageId = 10,
                IsDisabled = false,
                Lines = [new AccountLine { Name = "b", User = alice, UserId = 1, Value = 12 }]
            },
            new()
            {
                PageId = 10,
                IsDisabled = true, // doit être ignorée
                Lines = [new AccountLine { Name = "c", User = bob, UserId = 2, Value = 999 }]
            }
        };

        // CalculPaymentDonesFromEnters est une fonction pure : elle n'utilise pas le contexte.
        var business = new PaymentDoneBusiness(null!);

        var result = business.CalculPaymentDonesFromEnters(enters);

        Assert.Equal(2, result.Count);
        Assert.Equal(42d, (double)result.Single(p => p.UserId == 1).Total, 2);
        Assert.Equal(20d, (double)result.Single(p => p.UserId == 2).Total, 2);
        Assert.All(result, p => Assert.Equal(10, p.PageId));
        Assert.All(result, p => Assert.False(p.PaymentIsDone));
    }

    [Fact]
    public void CalculPaymentDonesFromEnters_GroupsPerUserAndPage()
    {
        var alice = new User { Id = 1, UserName = "Alice" };

        var enters = new List<AccountEnter>
        {
            new()
            {
                PageId = 10,
                Lines = [new AccountLine { Name = "a", User = alice, UserId = 1, Value = 30 }]
            },
            new()
            {
                PageId = 20, // page différente -> règlement distinct pour le même membre
                Lines = [new AccountLine { Name = "b", User = alice, UserId = 1, Value = 50 }]
            }
        };

        var business = new PaymentDoneBusiness(null!);

        var result = business.CalculPaymentDonesFromEnters(enters);

        Assert.Equal(2, result.Count);
        Assert.Equal(30d, (double)result.Single(p => p.PageId == 10).Total, 2);
        Assert.Equal(50d, (double)result.Single(p => p.PageId == 20).Total, 2);
        Assert.All(result, p => Assert.Equal(1, p.UserId));
    }

    [Fact]
    public void CalculPaymentDonesFromEnters_NoEnters_ReturnsEmpty()
    {
        var business = new PaymentDoneBusiness(null!);

        var result = business.CalculPaymentDonesFromEnters([]);

        Assert.Empty(result);
    }
}
