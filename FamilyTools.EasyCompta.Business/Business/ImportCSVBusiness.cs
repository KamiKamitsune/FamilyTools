using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

namespace FamilyTools.EasyCompta.Business;
public partial class ImportCSVBusiness(
    IAccountPageBusiness accountPageBusiness,
    IUserBusiness userBusiness,
    IAccountTagBusiness accountTagBusiness,
    IAccountEnterBusiness accountEnterBusiness,
    ILogger<ImportCSVBusiness> logger,
    IOperationTypeBusiness operationTypeBusiness) : IImportCSVBusiness
{
    private readonly IAccountPageBusiness _accountPageBusiness = accountPageBusiness;
    private readonly IAccountTagBusiness _accountTagBusiness = accountTagBusiness;
    private readonly IAccountEnterBusiness _accountEnterBusiness = accountEnterBusiness;
    private readonly ILogger<ImportCSVBusiness> _logger = logger;
    private readonly IUserBusiness _userBusiness = userBusiness;
    private readonly IOperationTypeBusiness _operationTypeBusiness = operationTypeBusiness;

    public async Task CSVToAccountPages(byte[] csvFile)
    {
        var csvContent = await ReadCSV(csvFile);
        csvContent = CleanCSVContent(csvContent);
        var csvCAs = await this.ConvertCSVToCSVCA(csvContent);
        var accountEnters = await this.ChangeInAccountEnter(csvCAs);
        await this.PostToBusiness(accountEnters);
    }

    private async Task PostToBusiness(List<AccountEnter> accountEnters)
    {
        var accountEnterGroupByMouthAndYear = accountEnters.GroupBy(x => new { x.Date.Year, x.Date.Month })
            .Select(x => new Tuple<int, int, List<AccountEnter>>(x.Key.Year, x.Key.Month, [.. x]))
            .ToList();

        List<AccountPage> pages = [];
        foreach (var accountEnter in accountEnterGroupByMouthAndYear)
            pages.Add(new AccountPage(accountEnter.Item3, new DateOnly(accountEnter.Item1, accountEnter.Item2, 1)));

        await this._accountPageBusiness.CreateOrUpdateListPage(pages);
    }

    private async Task<List<AccountEnter>> ChangeInAccountEnter(List<CSVCA> csvCAs)
    {
        var accountEnters = new List<AccountEnter>();
        var defaultTag = await this._accountTagBusiness.DefaultTag() ?? new AccountTag();
        // Tags suivis par le contexte : on les réutilise tels quels pour qu'EF ne les ré-insère pas.
        var tagById = (await this._accountTagBusiness.TagList()).ToDictionary(tag => tag.Id);
        // Auto-tag : un libellé déjà tagué à la main applique son tag aux prochains imports homonymes.
        var tagIdByName = await this._accountEnterBusiness.TagIdByName(defaultTag.Id);
        var users = await this._userBusiness.UserList();

        foreach (var csvca in csvCAs)
        {
            float totalValue = 0;
            totalValue -= csvca.Credit;
            totalValue += csvca.Debit;

            var lines = AccountLineSplitter.SplitEqually(
                totalValue,
                users,
                _ => $"{csvca.Libelle}_{DateOnly.FromDateTime(csvca.Date)}");

            var tag = tagIdByName.TryGetValue(csvca.Libelle, out var tagId)
                      && tagById.TryGetValue(tagId, out var matchedTag)
                ? matchedTag
                : defaultTag;

            accountEnters.Add(new AccountEnter(csvca.Libelle, csvca.OperationType,
                DateOnly.FromDateTime(csvca.Date),
                totalValue, lines, tag));
        }

        return accountEnters;
    }

    private async Task<List<CSVCA>> ConvertCSVToCSVCA(string csvContent)
    {
        var csvCAs = new List<CSVCA>();

        try
        {
            using var reader = new StringReader(csvContent);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                MissingFieldFound = null,
                BadDataFound = context =>
                {
                    this._logger.LogWarning($"Bad data found at row {context.Field}: {context.RawRecord}");
                },
                // Configuration supplémentaire pour éviter les erreurs
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                // Permettre les champs avec des guillemets
                Quote = '"',
                Escape = '"',
                // IMPORTANT: Permettre les champs multi-lignes
                DetectDelimiter = false,
                LineBreakInQuotedFieldIsBadData = false, // Crucial !
                Mode = CsvMode.RFC4180 // Mode standard qui gère les multi-lignes
            };

            using var r = new CsvReader(reader, config);

            while (await r.ReadAsync())
                try
                {
                    // Vérifier qu'on a assez de champs
                    if (r.Parser.Count < 3)
                    {
                        this._logger.LogWarning(
                            $"Row {r.Context.Parser.Row} has insufficient fields: {r.Context.Parser.RawRecord}");
                        continue;
                    }

                    var dateStr = r.GetField(0)?.Trim();
                    var libelle = r.GetField(1)?.Trim();
                    var debitStr = r.GetField(2)?.Trim();
                    //var creditStr = r.Parser.Count > 3 ? r.GetField(3)?.Trim() : "";
                    var creditStr = r.GetField(3)?.Trim();


                    // Nettoyer le libellé des espaces multiples et sauts de ligne
                    if (!string.IsNullOrEmpty(libelle)) libelle = Regex.Replace(libelle, @"\s+", " ").Trim();

                    // Extraire le type
                    var type = ExtractOperationTypeAsync(libelle);

                    libelle = CleanLibelle(libelle);
                    var csvca = new CSVCA
                    {
                        Date = this.ParseDate(dateStr),
                        Libelle = libelle,
                        OperationType = await type,
                        Debit = ParseAmount(debitStr),
                        Credit = ParseAmount(creditStr)
                    };

                    // Ne pas ajouter les transactions invalides
                    if (csvca.Date != DateTime.MinValue) csvCAs.Add(csvca);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning(
                        $"Error parsing row {r.Context.Parser.Row}: {ex.Message} - Raw: {r.Context.Parser.RawRecord}");
                }

            this._logger.LogInformation($"Successfully parsed {csvCAs.Count} transactions");
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error parsing CSV content: {ex.Message}");
            throw;
        }

        return csvCAs;
    }

    private static async Task<string> ReadCSV(byte[] csvFile)
    {
        using var stream = new MemoryStream(csvFile);
        using var reader = new StreamReader(stream, Encoding.GetEncoding("Windows-1252"));
        return await reader.ReadToEndAsync();
    }
    //enregistrer le fichier

    internal static string CleanCSVContent(string csvContent)
    {
        if (string.IsNullOrEmpty(csvContent)) return csvContent;

        csvContent = RemoveFirstLines(csvContent, 11);
        csvContent = CleanSeparators(csvContent);
        csvContent = RemoveEmptyLines(csvContent);

        return csvContent;
    }

    internal static string RemoveFirstLines(string csvContent, int linesToSkip)
    {
        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.None);
        var filteredLines = lines.Skip(linesToSkip).Where(line => !string.IsNullOrEmpty(line.Trim()));
        return string.Join(Environment.NewLine, filteredLines);
    }

    internal static string CleanSeparators(string csvContent)
    {
        // S'assurer que les lignes se terminent bien par des points-virgules
        // pour les colonnes vides (Débit ou Crédit)
        var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var cleanedLines = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cleanLine = line.Trim();

            // Compter les séparateurs
            var separatorCount = cleanLine.Count(c => c == ';');

            // Une ligne de transaction doit avoir 4 colonnes (3 séparateurs)
            // Date;Libellé;Débit;Crédit;
            if (separatorCount < 3)
                // Ajouter les séparateurs manquants
                while (separatorCount < 3)
                {
                    cleanLine += ";";
                    separatorCount++;
                }

            cleanedLines.Add(cleanLine);
        }

        return string.Join(Environment.NewLine, cleanedLines);
    }

    private static string RemoveEmptyLines(string csvContent)
    {
        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }


    private DateTime ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.MinValue;

        // Format attendu: DD/MM/YYYY
        if (DateTime.TryParseExact(dateStr.Trim(), "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        this._logger.LogWarning($"Unable to parse date: {dateStr}");
        return DateTime.MinValue;
    }

    internal static float ParseAmount(string? amountStr)
    {
        if (string.IsNullOrWhiteSpace(amountStr)) return 0;

        // Nettoyer l'amount (supprimer espaces, remplacer virgule par point)
        //var cleanAmount = amountStr.Trim()
        //    .Replace(" ", "")
        //    .Replace(",", ".");
        var cleanAmount = IncecableSpace().Replace(amountStr.Trim(), "").Replace(",", ".");

        if (float.TryParse(cleanAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)) return amount;

        return 0;
    }

    internal static string CleanLibelle(string libelleRaw)
    {
        if (string.IsNullOrWhiteSpace(libelleRaw)) return string.Empty;

        var parts = libelleRaw.Split(new[] { ";;" }, StringSplitOptions.None);

        if (parts.Length < 2) return libelleRaw;

        var result = parts[1].Replace(";", "").Trim();
        result = MultiSpace().Replace(result, " ");
        result = XAndNumbers().Replace(result, "");
        result = FormatDateDDMM().Replace(result, "");

        return result;
    }

    private async Task<OperationType> ExtractOperationTypeAsync(string? libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle)) return new OperationType { Id = 0, Name = "Unknown" };

        libelle = libelle.Split(';').First();

        var operationType = await this._operationTypeBusiness.FindByName(libelle);

        if (operationType == default || operationType.Id == 0)
        {
            operationType = await this._operationTypeBusiness.Create(new OperationType { Name = libelle });
        }

        return operationType;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpace();

    [GeneratedRegex(@"^X\d+\s+")]
    private static partial Regex XAndNumbers();

    [GeneratedRegex(@"\s+\d{2}/\d{2}\s*$")]
    private static partial Regex FormatDateDDMM();

    [GeneratedRegex(@"[\s.\u00A0\u202F\u2009]")]
    private static partial Regex IncecableSpace();
}
