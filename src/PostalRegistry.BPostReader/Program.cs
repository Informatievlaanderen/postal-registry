namespace PostalRegistry.BPostReader
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Data;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NodaTime;
    using PostalInformation.Commands.BPost;

    public class Program
    {
        private const string ImportedPathConfigKey = "importedPath";
        private const string BpostUrlConfigKey = "bpostUrl";
        private const string ImportUrlConfigKey = "importUrl";
        private const string FileDateFormat = "yyyyMMdd_HHmmss";

        private static IConfigurationRoot _configuration;
        private static string _importPath;

        public static void Main(string[] args)
        {
            var configureForPostalRegistry = JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureForPostalRegistry();
            JsonConvert.DefaultSettings = () => configureForPostalRegistry;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .Build();

            _importPath = string.IsNullOrEmpty(_configuration[ImportedPathConfigKey])
                ? AppDomain.CurrentDomain.BaseDirectory + "\\imported"
                : _configuration[ImportedPathConfigKey];

            Directory.CreateDirectory(_importPath);

            var importInstant = Instant.FromDateTimeOffset(DateTimeOffset.Now);
            var timestamp = new BPostTimestamp(importInstant);

            var dataToImport = DownloadPostalCodes();
            var previouslyImportedData = GetPreviouslyImportedData();

            var commandFactory = new CommandFactory(GetPostalNamesWithLanguage());

            var oldByPostalCode = previouslyImportedData.GroupBy(x => x.PostalCode).ToDictionary(x => x.Key, x => x.ToList());
            var newByPostalCode = dataToImport.GroupBy(x => x.PostalCode).ToDictionary(x => x.Key, x => x.ToList());

            ImportCommands(GenerateCommands(newByPostalCode, oldByPostalCode, commandFactory, timestamp));

            Console.WriteLine("Saving imported html to csv");
            CsvWriterHelper.Export($"{_importPath}\\{importInstant.InUtc().ToString(FileDateFormat, CultureInfo.InvariantCulture)}.csv", dataToImport);

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void ImportCommands(IReadOnlyCollection<ImportPostalInformationFromBPost> commands)
        {
            if (!commands.Any())
                return;

            Console.WriteLine("Press key to start import");
            Console.ReadKey();

            Parallel.ForEach(
                commands,
                new ParallelOptions { MaxDegreeOfParallelism = 8 },
                commmand =>
                {
                    SendBPostImportCommand(
                        commmand.PostalCode,
                        commmand,
                        commmand.CreateCommandId());
                });
        }

        private static List<ImportPostalInformationFromBPost> GenerateCommands(
            Dictionary<string, List<BPostData>> newByPostalCode,
            Dictionary<string, List<BPostData>> oldByPostalCode,
            CommandFactory commandFactory,
            BPostTimestamp timestamp)
        {
            var commands = new List<ImportPostalInformationFromBPost>();
            commands.AddRange(GenerateNewCommands(newByPostalCode, oldByPostalCode, commandFactory, timestamp));
            commands.AddRange(GenerateDeleteCommands(oldByPostalCode, newByPostalCode, commandFactory, timestamp));
            commands.AddRange(GenerateUpdateCommands(newByPostalCode, oldByPostalCode, commandFactory, timestamp));
            Console.WriteLine($"Generated {commands.Count} commands");
            return commands;
        }

        private static IEnumerable<ImportPostalInformationFromBPost> GenerateNewCommands(
            Dictionary<string, List<BPostData>> newByPostalCode,
            Dictionary<string, List<BPostData>> oldByPostalCode,
            CommandFactory commandFactory,
            BPostTimestamp timestamp)
        {
            // DETERMINE NEW
            var newKeys = newByPostalCode.Keys.Except(oldByPostalCode.Keys);
            var commands = newKeys.Select(newKey => commandFactory.Create(newByPostalCode[newKey], timestamp, Modification.Insert)).ToList();
            Console.WriteLine($"Generated {commands.Count} insert commands");
            return commands;
        }

        private static IEnumerable<ImportPostalInformationFromBPost> GenerateDeleteCommands(
            Dictionary<string, List<BPostData>> oldByPostalCode,
            Dictionary<string, List<BPostData>> newByPostalCode,
            CommandFactory commandFactory,
            BPostTimestamp timestamp)
        {
            // DETERMINE DELETED
            var deletedKeys = oldByPostalCode.Keys.Except(newByPostalCode.Keys);
            var commands = deletedKeys.Select(key => commandFactory.Create(oldByPostalCode[key], timestamp, Modification.Delete)).ToList();
            Console.WriteLine($"Generated {commands.Count} delete commands");
            return commands;
        }

        private static IEnumerable<ImportPostalInformationFromBPost> GenerateUpdateCommands(
            Dictionary<string, List<BPostData>> newByPostalCode,
            Dictionary<string, List<BPostData>> oldByPostalCode,
            CommandFactory commandFactory,
            BPostTimestamp timestamp)
        {
            // DETERMINE UPDATED
            var commands = new List<ImportPostalInformationFromBPost>();
            var updateKeys = newByPostalCode.Keys.Intersect(oldByPostalCode.Keys);

            foreach (var updateKey in updateKeys)
            {
                var old = oldByPostalCode[updateKey];
                var oldRoot = old.GetRootRecord();
                var oldPostalNames = old.Select(x => x.PostalName).ToList();

                var @new = newByPostalCode[updateKey];
                var newRoot = @new.GetRootRecord();
                var newPostalNames = @new.Select(x => x.PostalName).ToList();

                if (oldRoot.IsSubMunicipality != newRoot.IsSubMunicipality ||
                    !string.Equals(oldRoot.Province, newRoot.Province, StringComparison.OrdinalIgnoreCase) ||
                    oldPostalNames.Any(x => !newPostalNames.Contains(x)) ||
                    newPostalNames.Any(x => !oldPostalNames.Contains(x)))
                {
                    commands.Add(commandFactory.Create(@new, timestamp, Modification.Update));
                }
            }

            Console.WriteLine($"Generated {commands.Count} update commands");
            return commands;
        }

        private static List<BPostData> DownloadPostalCodes()
        {
            Console.WriteLine($"Downloading BPost postal codes from {_configuration[BpostUrlConfigKey]}");

            string htmlString;
            using (var webClient = new WebClient())
                htmlString = webClient.DownloadString(_configuration[BpostUrlConfigKey]);

            Console.WriteLine("Downloaded postal codes");

            var bpostData = CreateBPostDataFrom(htmlString);

            Console.WriteLine("Converted downloaded postal codes");

            return bpostData;
        }

        private static List<BPostData> CreateBPostDataFrom(string htmlString)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htmlString);

            var rows = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='sheet0']")
                .Descendants("tr")
                .Skip(2); //Header + empty row

            return rows
                .Select(row => row.Elements("td")
                    .Select(td => td.InnerText.Trim())
                    .ToList())
                .Select(columns => new BPostData
                {
                    PostalCode = columns[0],
                    PostalName = HttpUtility.HtmlDecode(columns[1]),
                    IsSubMunicipality = GetSubmunicipalityFromColumn(columns[2]),
                    Province = HttpUtility.HtmlDecode(columns[3]),
                })
                .ToList();
        }

        private static bool? GetSubmunicipalityFromColumn(string column)
        {
            bool? isSubmunicipality = null;

            if (!string.IsNullOrWhiteSpace(column))
                isSubmunicipality = string.Equals(column, "Ja", StringComparison.OrdinalIgnoreCase);

            return isSubmunicipality;
        }

        private static IEnumerable<BPostData> GetPreviouslyImportedData()
        {
            var importedFiles = Directory.GetFiles(_importPath, "*.csv");
            var latestFile = string.Empty;
            var latestImport = DateTimeOffset.MinValue;

            foreach (var importedFile in importedFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(importedFile);
                var timeStamp = DateTimeOffset.ParseExact(fileName, FileDateFormat, CultureInfo.InvariantCulture);

                // If the file date is smaller than our last import, we skip it
                if (timeStamp <= latestImport)
                    continue;

                latestImport = timeStamp;
                latestFile = importedFile;
            }

            return string.IsNullOrEmpty(latestFile)
                ? new List<BPostData>()
                : CsvReaderHelper.ReadBPostData(latestFile);
        }

        private static IEnumerable<PostalName> GetPostalNamesWithLanguage()
        {
            var filePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\postalNamesLanguage.csv";
            return CsvReaderHelper.ReadPostalNames(filePath);
        }

        private static void SendBPostImportCommand<T>(string id, T command, Guid commandId)
        {
            var type = typeof(T).FullName;
            var commandJson = JsonConvert.SerializeObject(command);
            var request = new { type, bpostItem = commandJson };
            var json = JsonConvert.SerializeObject(request);

            var username = _configuration["AuthUserName"];
            var password = _configuration["AuthPassword"];

            using (var client = new HttpClient { BaseAddress = new Uri(_configuration[ImportUrlConfigKey]) })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    var encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedString);
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("CommandId", commandId.ToString("D"));

                Console.Write($"Posting postal code: {id} ");

                var response = client.PostAsync("v1/bpostimport", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                Console.WriteLine("[OK]");
            }
        }
    }
}
