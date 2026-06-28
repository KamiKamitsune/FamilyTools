using System.Runtime.CompilerServices;
using System.Text;

namespace FamilyTools.EasyCompta.Business;

internal static class EncodingBootstrap
{
    /// <summary>
    /// .NET (Core+) ne fournit pas les code pages héritées (Windows-1252, etc.) sans ce provider.
    /// Les relevés bancaires CSV étant encodés en Windows-1252, on l'enregistre dès le chargement
    /// de la couche métier, quel que soit l'hôte (API ou worker).
    /// </summary>
    [ModuleInitializer]
    internal static void Register()
        => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}
