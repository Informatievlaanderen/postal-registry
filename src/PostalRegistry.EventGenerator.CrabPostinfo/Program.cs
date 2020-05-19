namespace PostalRegistry.EventGenerator.CrabPostinfo
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using PostalInformation.Commands.Crab;

    public class Program
    {
        private const string ImportUrlConfigKey = "importUrl";
        private const string CrabConnectionStringConfigKey = "crabConnectionString";

        private static IConfigurationRoot _configuration;

        public static void Main(string[] args)
        {
            var configureForPostalRegistry = JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureForPostalRegistry();
            JsonConvert.DefaultSettings = () => configureForPostalRegistry;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .Build();

            Console.WriteLine("Reading postal info from crab");

            var connectionString = _configuration[CrabConnectionStringConfigKey];
            var commands = new List<ImportPostalInformationFromCrab>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var fromDateTimeOffset = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                using (var cmd = new SqlCommand("SELECT [subKantonId], [postkantonCode], [subkantonCode], [BeginDatum], [nisGemeenteCode], [gemeenteNaam] FROM odb.vwSubKantonCodes", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            commands.Add(
                                new ImportPostalInformationFromCrab(
                                    new PostalCode(reader.GetString(1)),
                                    new CrabSubCantonId(reader.GetInt32(0)),
                                    new CrabSubCantonCode(reader.GetString(2)),
                                    new NisCode(reader.GetString(4)),
                                    new CrabMunicipalityName(reader.GetString(5), null),
                                    new CrabLifetime(reader.GetDateTime(3).ToCrabLocalDateTime(), null),
                                    new CrabTimestamp(fromDateTimeOffset),
                                    new CrabOperator("VLM\\PostalRegistry.EventGenerator.CrabPostinfo"),
                                    CrabModification.Correction,
                                    CrabOrganisation.DePost));
                        }
                    }
                }
            }

            Console.WriteLine($"Generated {commands.Count} commands");
            Console.WriteLine("Press key to start import");
            Console.ReadKey();

            foreach (var importPostalInfoFromCrab in commands)
                SendCrabImportCommand(importPostalInfoFromCrab.PostalCode, importPostalInfoFromCrab, importPostalInfoFromCrab.CreateCommandId());

            Console.WriteLine("Finished");
        }

        private static void SendCrabImportCommand<T>(string id, T command, Guid commandId)
        {
            var type = typeof(T).FullName;
            var commandJson = JsonConvert.SerializeObject(command);
            var request = new { type, crabItem = commandJson };
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

                var response = client.PostAsync("v1/crabimport", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                Console.WriteLine("[OK]");
            }
        }
    }
}
