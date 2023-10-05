namespace PostalRegistry.Api.Oslo.Nuts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CsvHelper;
    using CsvHelper.Configuration;

    public sealed class Nuts3Service
    {
        private const string Nuts3FileName = "nuts3_postal_v1.csv";
        private readonly List<Nuts3Record> _nuts3Records = new List<Nuts3Record>();

        public Nuts3Service()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nuts", Nuts3FileName);

            if (!File.Exists(filePath))
            {
                // Handle the case where the file does not exist
                throw new FileNotFoundException("NUTS3 CSV file not found.");
            }

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture){
                Delimiter = ";",
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8,
                Quote = '\''
            };

            using var reader = new StreamReader(filePath);
            using var csvReader = new CsvReader(reader, csvConfiguration);
            csvReader.Read(); // skip first record Header as ReadHeader doesn't work

            while (csvReader.Read())
            {
                var nuts3Code = csvReader.GetField<string>(0);
                var postalCode = csvReader.GetField<string>(1);

                _nuts3Records.Add(new Nuts3Record(nuts3Code, postalCode));
            }
        }

        public Nuts3Record? GetNuts3ByPostalCode(string postalCode)
        {
            return _nuts3Records.FirstOrDefault(x => x.PostalCode == postalCode);
        }

        public IEnumerable<Nuts3Record> GetPostalCodesByNuts3(string nuts3Code)
        {
            return _nuts3Records.Where(x => string.Equals(x.Nuts3Code, nuts3Code, StringComparison.OrdinalIgnoreCase));
        }
    }

    public record Nuts3Record(string Nuts3Code, string PostalCode);
}
