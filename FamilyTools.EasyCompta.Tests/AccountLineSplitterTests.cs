using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.Business;

namespace FamilyTools.EasyCompta.Tests;

public class AccountLineSplitterTests
{
    private static List<User> Users(int n) =>
        Enumerable.Range(1, n).Select(i => new User { Id = i, UserName = $"U{i}" }).ToList();

    [Fact]
    public void SplitEqually_NonDivisible_PutsRemainderOnFirstLine_AndSumMatchesTotal()
    {
        var lines = AccountLineSplitter.SplitEqually(100f, Users(3), _ => "L");

        Assert.Equal(3, lines.Count);
        Assert.Equal(33.34, (double)lines[0].Value, 2); // reliquat de 0,01 sur la 1re ligne
        Assert.Equal(33.33, (double)lines[1].Value, 2);
        Assert.Equal(33.33, (double)lines[2].Value, 2);
        Assert.Equal(100.00, (double)lines.Sum(l => l.Value), 2);
    }

    [Fact]
    public void SplitEqually_Divisible_IsEqual()
    {
        var lines = AccountLineSplitter.SplitEqually(10f, Users(2), _ => "L");

        Assert.All(lines, l => Assert.Equal(5.00, (double)l.Value, 2));
        Assert.Equal(10.00, (double)lines.Sum(l => l.Value), 2);
    }

    [Fact]
    public void SplitEqually_NegativeTotal_SumMatchesTotal()
    {
        var lines = AccountLineSplitter.SplitEqually(-10f, Users(3), _ => "L");

        Assert.Equal(-10.00, (double)lines.Sum(l => l.Value), 2);
    }

    [Fact]
    public void SplitEqually_SetsUserUserIdAndName()
    {
        var users = Users(2);

        var lines = AccountLineSplitter.SplitEqually(10f, users, u => $"ligne-{u.Id}");

        Assert.Same(users[0], lines[0].User);
        Assert.Equal(1, lines[0].UserId);
        Assert.Equal("ligne-1", lines[0].Name);
    }

    [Fact]
    public void SplitEqually_NoUsers_ReturnsEmpty()
    {
        Assert.Empty(AccountLineSplitter.SplitEqually(100f, new List<User>(), _ => "L"));
    }
}
