namespace PostalRegistry.BPostReader.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CsvHelper;
    using CsvHelper.Configuration;

    public static class CsvReaderHelper
    {
        public static IEnumerable<BPostData> ReadBPostData(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = true, Encoding = Encoding.UTF8 });
                return csvReader.GetRecords<BPostData>().ToList();
            }
        }

        public static IEnumerable<PostalName> ReadPostalNames(string path)
        {
            var data = new List<PostalName>();

            using (var reader = new StreamReader(path))
            {
                var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 });
                while (csvReader.Read())
                {
                    var name = csvReader.GetField<string>(0);
                    var lang = csvReader.GetField<string>(1);

                    data.Add(new PostalName(name, MapIsoToLanguage(lang)));
                }
            }

            return data;
        }

        private static Language MapIsoToLanguage(string language)
        {
            switch (language.ToUpperInvariant())
            {
                case "NL":
                    return Language.Dutch;

                case "EN":
                    return Language.English;

                case "FR":
                    return Language.French;

                case "DE":
                    return Language.German;

                default:
                    throw new NotImplementedException($"Cannot map two letter iso {language} to Language");
            }
        }
    }
}
