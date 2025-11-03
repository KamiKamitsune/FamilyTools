
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


using CsvHelper;
using CsvHelper.Configuration;

using FamilyTools.CSVConvert.Interface;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.Business;

using Microsoft.AspNetCore.Http;

namespace FamilyTools.CSVConvert
{
    public partial class ConvertCSVCAToAccountPages(
        ILogger<ConvertCSVCAToAccountPages> logger, 
        AccountTagBusiness accountTagBusiness, 
        UserBusiness userBusiness,
        AccountPageBusiness accountPageBusiness) 
        : IConvertCSVCAToAccountPages
    {
        private readonly ILogger<ConvertCSVCAToAccountPages> _logger = logger;

        private readonly AccountTagBusiness _accountTagBusiness = accountTagBusiness;
        private readonly UserBusiness _userBusiness = userBusiness;
        private readonly AccountPageBusiness _accountPageBusiness = accountPageBusiness;


        private string _csvContent = "";
        private List<CSVCA> _csvCAs = [];
        private List<AccountEnter> _accountEnters = [];

        private CancellationToken _cancellationToken;

        public async Task ConvertCSVFile(IFormFile csvFile, CancellationToken cancellationToken)
        {
            if (csvFile == null) return;

            this._cancellationToken = cancellationToken;

            if (this._cancellationToken.IsCancellationRequested) return;
            await this.ReadCSV(csvFile);

            if (this._cancellationToken.IsCancellationRequested) return;
            this.CleanCSVContent();

            if (this._cancellationToken.IsCancellationRequested) return;
            await this.ConvertCSVToCSVCA();

            if (this._cancellationToken.IsCancellationRequested) return;
            await this.ChangeInAccountEnter();

            if (this._cancellationToken.IsCancellationRequested) return;
            await this.PostToBusiness();
        }

        private async Task PostToBusiness()
        {
            var accountEnterGroupByMouthAndYear = _accountEnters.GroupBy(x => new { x.Date.Year, x.Date.Month })
                 .Select(x => new Tuple<int, int, List<AccountEnter>>(x.Key.Year, x.Key.Month, [.. x]))
                 .ToList();

            //TODO: Créer des account page par rapport au mois et a l'année si elle n'existe pas, sinon les ajoutées à des pages existante (create or update dans la page business?)
            List<AccountPage> pages = [];
            foreach (var accountEnter in accountEnterGroupByMouthAndYear)
            {
                if (this._cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                pages.Add(new AccountPage(accountEnter.Item3, new DateOnly(accountEnter.Item1, accountEnter.Item2, 1)));
            }

            await this._accountPageBusiness.CreateListPage(pages);

        }

        private async Task ChangeInAccountEnter()
        {
            var defaultTag = await this._accountTagBusiness.DefaultTag() ?? new AccountTag();
            foreach (var csvca in _csvCAs)
            {
                if (this._cancellationToken.IsCancellationRequested) return;

                float totalValue = 0;
                totalValue += csvca.Credit;
                totalValue += csvca.Debit;

                var users = await this._userBusiness.UserList();

                List<AccountLine> lines = [];

                foreach (var user in users)
                {
                    lines.Add(new AccountLine()
                    {
                        Name = $"{csvca.Libelle}_{DateOnly.FromDateTime(csvca.Date)}",
                        User = user,
                        Value = totalValue / users.Count,
                    });
                }

                _accountEnters.Add(new AccountEnter(csvca.Libelle, DateOnly.FromDateTime(csvca.Date), totalValue, lines, defaultTag));
            }
        }

        private async Task ConvertCSVToCSVCA()
        {

            try
            {
                using var reader = new StringReader(this._csvContent);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false,
                    MissingFieldFound = null,
                    BadDataFound = context =>
                    {
                        _logger.LogWarning($"Bad data found at row {context.Field}: {context.RawRecord}");
                    },
                    // Configuration supplémentaire pour éviter les erreurs
                    IgnoreBlankLines = true,
                    TrimOptions = TrimOptions.Trim,
                    // Permettre les champs avec des guillemets
                    Quote = '"',
                    Escape = '"'
                };

                using var r = new CsvReader(reader, config);

                while (await r.ReadAsync())
                {
                    if (this._cancellationToken.IsCancellationRequested) return;

                    try
                    {
                        // Vérifier qu'on a assez de champs
                        if (r.Parser.Count < 3)
                        {
                            _logger.LogWarning($"Row {r.Context.Parser.Row} has insufficient fields: {r.Context.Parser.RawRecord}");
                            continue;
                        }

                        var csvca = new CSVCA
                        {
                            Date = ParseDate(r.GetField(0)),
                            Libelle = CleanDescription(r.GetField(1)),
                            Debit = ParseAmount(r.GetField(2)),
                            Credit = r.Parser.Count > 3 ? ParseAmount(r.GetField(3)) : 0
                        };

                        // Ne pas ajouter les transactions invalides
                        if (csvca.Date != DateTime.MinValue)
                        {
                            this._csvCAs.Add(csvca);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error parsing row {r.Context.Parser.Row}: {ex.Message} - Raw: {r.Context.Parser.RawRecord}");
                    }
                }

                _logger.LogInformation($"Successfully parsed {_csvCAs.Count} transactions");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing CSV content: {ex.Message}");
                throw;
            }

        }

        private async Task ReadCSV(IFormFile csvFile)
        {
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.GetEncoding("Windows-1252"));
            this._csvContent = await reader.ReadToEndAsync();
        }
        //enregistrer le fichier

        private void CleanCSVContent()
        {
            if (string.IsNullOrEmpty(_csvContent)) return;

            RemoveFirstLines(11);
            CleanInvalidLineBreaks().Replace(this._csvContent, " ");
            RemoveMultiSpace().Replace(this._csvContent, " ");
            CleanSeparators();
            RemoveEmptyLines();
        }

        private void RemoveFirstLines(int linesToSkip)
        {

            var lines = this._csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var filteredLines = lines.Skip(linesToSkip).Where(line => !string.IsNullOrEmpty(line.Trim()));
            this._csvContent = string.Join(Environment.NewLine, filteredLines);
        }

        private void CleanSeparators()
        {
            // S'assurer que les lignes se terminent bien par des points-virgules
            // pour les colonnes vides (Débit ou Crédit)
            var lines = this._csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                if (this._cancellationToken.IsCancellationRequested) return;

                if (string.IsNullOrWhiteSpace(line)) continue;

                var cleanLine = line.Trim();

                // Compter les séparateurs
                int separatorCount = cleanLine.Count(c => c == ';');

                // Une ligne de transaction doit avoir 4 colonnes (3 séparateurs)
                // Date;Libellé;Débit;Crédit;
                if (separatorCount < 3)
                {
                    // Ajouter les séparateurs manquants
                    while (separatorCount < 3)
                    {
                        cleanLine += ";";
                        separatorCount++;
                    }
                }

                cleanedLines.Add(cleanLine);
            }

            this._csvContent = string.Join(Environment.NewLine, cleanedLines);
        }

        private void RemoveEmptyLines()
        {
            var lines = this._csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            this._csvContent = string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        [GeneratedRegex(@" {2,}")]
        private static partial Regex RemoveMultiSpace();
        [GeneratedRegex(@"(?<!;)[\r\n]+")]
        private static partial Regex CleanInvalidLineBreaks();

        [GeneratedRegex(@"\s+")]
        private static partial Regex CleanDescriptionRegex();

        private DateTime ParseDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.MinValue;

            // Format attendu: DD/MM/YYYY
            if (DateTime.TryParseExact(dateStr.Trim(), "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            _logger.LogWarning($"Unable to parse date: {dateStr}");
            return DateTime.MinValue;
        }

        private static string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "";

            // Nettoyer la description en supprimant les espaces multiples et les guillemets
            return CleanDescriptionRegex().Replace(description.Trim().Trim('"'), " ");
        }

        private static float ParseAmount(string amountStr)
        {
            if (string.IsNullOrWhiteSpace(amountStr)) return 0;

            // Nettoyer l'amount (supprimer espaces, remplacer virgule par point)
            var cleanAmount = amountStr.Trim()
                .Replace(" ", "")
                .Replace("€", "")
                .Replace(",", ".");

            if (float.TryParse(cleanAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out float amount))
            {
                return amount;
            }

            return 0;
        }
    }
}
