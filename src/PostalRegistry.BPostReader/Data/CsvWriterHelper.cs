namespace PostalRegistry.BPostReader.Data
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;

    public static class CsvWriterHelper
    {
        public static void Export(string path, IEnumerable<BPostData> data)
        {
            using (var writer = new StreamWriter(path))
            {
                var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture, delimiter: ",", hasHeaderRecord: true));

                csvWriter.WriteHeader<BPostData>();
                csvWriter.NextRecord();
                csvWriter.WriteRecords(data);
            }
        }
    }
}
