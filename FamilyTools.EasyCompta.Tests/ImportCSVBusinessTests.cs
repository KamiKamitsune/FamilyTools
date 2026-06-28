using FamilyTools.EasyCompta.Business;

namespace FamilyTools.EasyCompta.Tests;

public class ImportCSVBusinessTests
{
    // --- ParseAmount : formats de montants des relevés bancaires français ---

    [Theory]
    [InlineData("12,50", 12.50)]
    [InlineData("1 234,56", 1234.56)] // séparateur de milliers (espace)
    [InlineData("-45,30", -45.30)]
    [InlineData("2000,00", 2000.00)]
    [InlineData("", 0.0)]
    [InlineData("   ", 0.0)]
    [InlineData("abc", 0.0)]
    public void ParseAmount_ParsesFrenchAmounts(string input, double expected)
    {
        var result = ImportCSVBusiness.ParseAmount(input);

        Assert.Equal(expected, (double)result, 2);
    }

    [Fact]
    public void ParseAmount_RemovesNonBreakingSpace()
    {
        // Les relevés CA utilisent des espaces insécables (U+00A0) comme séparateurs de milliers
        var nbsp = ((char)0x00A0).ToString();

        var result = ImportCSVBusiness.ParseAmount($"1{nbsp}234,56");

        Assert.Equal(1234.56, (double)result, 2);
    }

    // --- CleanLibelle ---

    [Fact]
    public void CleanLibelle_ExtractsSecondSegmentAndCleans()
    {
        // 2e segment après ";;", retrait du préfixe "X<n> " et de la date " dd/MM" finale
        var result = ImportCSVBusiness.CleanLibelle("HEADER;;X12 ACHAT MAGASIN 01/02");

        Assert.Equal("ACHAT MAGASIN", result);
    }

    [Fact]
    public void CleanLibelle_WithoutDoubleSeparator_ReturnsInputUnchanged()
    {
        Assert.Equal("PAIEMENT CB", ImportCSVBusiness.CleanLibelle("PAIEMENT CB"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CleanLibelle_Blank_ReturnsEmpty(string input)
    {
        Assert.Equal(string.Empty, ImportCSVBusiness.CleanLibelle(input));
    }

    // --- Nettoyage du contenu CSV ---

    [Fact]
    public void CleanSeparators_PadsLineToThreeSeparators()
    {
        // Une ligne de transaction doit avoir 3 séparateurs (Date;Libellé;Débit;Crédit)
        Assert.Equal("a;b;;", ImportCSVBusiness.CleanSeparators("a;b"));
    }

    [Fact]
    public void CleanSeparators_KeepsCompleteLine()
    {
        Assert.Equal("a;b;c;d", ImportCSVBusiness.CleanSeparators("a;b;c;d"));
    }

    [Fact]
    public void RemoveFirstLines_SkipsRequestedLines()
    {
        var input = "l1\nl2\nl3\nl4";
        var expected = "l3" + Environment.NewLine + "l4";

        Assert.Equal(expected, ImportCSVBusiness.RemoveFirstLines(input, 2));
    }

    [Fact]
    public void CleanCSVContent_SkipsHeaderPadsAndRemovesEmptyLines()
    {
        // Le format CA comporte 11 lignes d'en-tête à ignorer
        var input = string.Join("\n",
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8", "h9", "h10", "h11",
            "01/01/2024;ACHAT;10,00;",
            "02/01/2024;SALAIRE;;2000,00");

        var expected = "01/01/2024;ACHAT;10,00;"
            + Environment.NewLine
            + "02/01/2024;SALAIRE;;2000,00";

        Assert.Equal(expected, ImportCSVBusiness.CleanCSVContent(input));
    }
}
