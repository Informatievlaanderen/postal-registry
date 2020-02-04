namespace PostalRegistry.BPostReader.Data
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using CsvHelper;

    public static class CsvWriterHelper
    {
        public static void Export(string path, IEnumerable<BPostData> data)
        {
            using (var writer = new StreamWriter(path))
            {
                var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture)
                {
                    Configuration =
                    {
                        Delimiter = ",",
                        HasHeaderRecord = true,
                    }
                };

                csvWriter.WriteHeader<BPostData>();
                csvWriter.NextRecord();
                csvWriter.WriteRecords(data);
            }
        }
    }
}
