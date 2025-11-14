namespace FamilyTools.EasyCompta.Business
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    using CsvHelper;
    using CsvHelper.Configuration;

    using FamilyTools.Data.Models.EasyCompta;
    using FamilyTools.EasyCompta.IBusiness;
    using FamilyTools.EasyCompta.Services;

    using static FamilyTools.Data.Models.EasyCompta.EnumEasycompta;

    public partial class ImportCSVBusiness(
        IAccountPageBusiness accountPageBusiness, 
        IUserBusiness userBusiness, 
        IAccountTagBusiness accountTagBusiness, 
        ILogger<ImportCSVBusiness> logger,
        IBackgroundCSVConvert taskQueue) : IImportCSVBusiness
    {
        private readonly IBackgroundCSVConvert _taskQueue = taskQueue;
        private readonly IAccountPageBusiness _accountPageBusiness = accountPageBusiness;
        private readonly IUserBusiness _userBusiness = userBusiness;
        private readonly IAccountTagBusiness _accountTagBusiness = accountTagBusiness;
        private readonly ILogger<ImportCSVBusiness> _logger = logger;

        private string _csvContent = "";
        private List<CSVCA> _csvCAs = [];
        private List<AccountEnter> _accountEnters = [];

        public async Task<bool> ImportCSVFile(IFormFile csvFile) {

            if (csvFile == null || csvFile.Length == 0)
                return false;

            // 🔹 On copie le contenu du fichier dans un MemoryStream
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await csvFile.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            await this._taskQueue.AddWorkItemInQueueBackgroundAsync(fileBytes);

            return true;
        }

        public async Task CSVToAccountPages(byte[] csvFile)
        {
            await this.ReadCSV(csvFile);
            this.CleanCSVContent();
            await this.ConvertCSVToCSVCA();
            await this.ChangeInAccountEnter();
            await this.PostToBusiness();
        }

        private async Task PostToBusiness()
        {
            var accountEnterGroupByMouthAndYear = _accountEnters.GroupBy(x => new { x.Date.Year, x.Date.Month })
                 .Select(x => new Tuple<int, int, List<AccountEnter>>(x.Key.Year, x.Key.Month, [.. x]))
                 .ToList();

            //TODO: Créer des account page par rapport au mois et a l'année si elle n'existe pas, sinon les ajoutées à des pages existante
            List<AccountPage> pages = [];
            foreach (var accountEnter in accountEnterGroupByMouthAndYear)
            {
                pages.Add(new AccountPage(accountEnter.Item3, new DateOnly(accountEnter.Item1, accountEnter.Item2, 1)));
            }

            await this._accountPageBusiness.CreateListPage(pages);

        }

        private async Task ChangeInAccountEnter()
        {
            var defaultTag = await this._accountTagBusiness.DefaultTag() ?? new AccountTag(); 
            foreach (var csvca in _csvCAs)
            {
                float totalValue = 0;
                totalValue -= csvca.Credit;
                totalValue += csvca.Debit;

                var users = await this._userBusiness.UserList();

                List<AccountLine> lines = [];

                foreach (var user in users)
                {
                    lines.Add(new AccountLine() {
                        Name = $"{csvca.Libelle}_{DateOnly.FromDateTime(csvca.Date)}",
                        User = user,
                        Value = totalValue / users.Count,
                    });
                }

                _accountEnters.Add(new AccountEnter(csvca.Libelle, csvca.OperationType, DateOnly.FromDateTime(csvca.Date), totalValue, lines, defaultTag));
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
                    Escape = '"',
                    // IMPORTANT: Permettre les champs multi-lignes
                    DetectDelimiter = false,
                    LineBreakInQuotedFieldIsBadData = false, // Crucial !
                    Mode = CsvMode.RFC4180 // Mode standard qui gère les multi-lignes
                };

                using var r = new CsvReader(reader, config);

                while (await r.ReadAsync())
                {
                    try
                    {
                        // Vérifier qu'on a assez de champs
                        if (r.Parser.Count < 3)
                        {
                            _logger.LogWarning($"Row {r.Context.Parser.Row} has insufficient fields: {r.Context.Parser.RawRecord}");
                            continue;
                        }

                        var dateStr = r.GetField(0)?.Trim();
                        var libelle = r.GetField(1)?.Trim();
                        var debitStr = r.GetField(2)?.Trim();
                        //var creditStr = r.Parser.Count > 3 ? r.GetField(3)?.Trim() : "";
                        var creditStr = r.GetField(3)?.Trim();


                        // Nettoyer le libellé des espaces multiples et sauts de ligne
                        if (!string.IsNullOrEmpty(libelle))
                        {
                            libelle = Regex.Replace(libelle, @"\s+", " ").Trim();
                        }

                        // Extraire le type
                        var type = ExtractOperationType(libelle);

                        libelle = CleanLibelle(libelle);
                        var csvca = new CSVCA
                        {
                            Date = ParseDate(dateStr),
                            Libelle = libelle,
                            OperationType = type,
                            Debit = ParseAmount(debitStr),
                            Credit = ParseAmount(creditStr)
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

        private async Task ReadCSV(byte[] csvFile)
        {
            using var stream = new MemoryStream(csvFile);
            using var reader = new StreamReader(stream, Encoding.GetEncoding("Windows-1252"));
            this._csvContent = await reader.ReadToEndAsync();
        }
        //enregistrer le fichier

        private void CleanCSVContent()
        {
            if (string.IsNullOrEmpty(_csvContent)) return;

            RemoveFirstLines(11);
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

        private static float ParseAmount(string amountStr)
        {
            if (string.IsNullOrWhiteSpace(amountStr)) return 0;

            // Nettoyer l'amount (supprimer espaces, remplacer virgule par point)
            //var cleanAmount = amountStr.Trim()
            //    .Replace(" ", "")
            //    .Replace(",", ".");
            var cleanAmount = IncecableSpace().Replace(amountStr.Trim(), "").Replace(",", ".");

            if (float.TryParse(cleanAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out float amount))
            {
                return amount;
            }

            return 0;
        }

        private static string CleanLibelle(string libelleRaw)
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

        private static OperationType ExtractOperationType(string libelle)
        {
            if (string.IsNullOrWhiteSpace(libelle)) return OperationType.Unknown;

            var upper = libelle.ToUpperInvariant();

            if (upper.Contains("PRELEVEMENT")) return OperationType.Prelevement;
            if (upper.Contains("PAIEMENT PAR CARTE")) return OperationType.PaiementCarte;
            if (upper.Contains("VIREMENT EN VOTRE FAVEUR") || upper.Contains("VIREMENT RECU"))
                return OperationType.VirementRecu;
            if (upper.Contains("VIREMENT EMIS")) return OperationType.VirementEmis;
            if (upper.Contains("REMBOURSEMENT DE PRET") || upper.Contains("REMBOURSEMENT PRET"))
                return OperationType.RemboursementPret;
            if (upper.Contains("REGLEMENT")) return OperationType.Reglement;
            if (upper.Contains("AVOIR")) return OperationType.Avoir;
            if (upper.Contains("COTISATION")) return OperationType.Cotisation;
            if (upper.Contains("REMISE DE CHEQUE")) return OperationType.ChequeEmis;

            return OperationType.Unknown;
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
}
