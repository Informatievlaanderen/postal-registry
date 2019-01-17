namespace PostalRegistry.BPostReader.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // DO NOT CHANGE PROPERTY NAMES => New Version of POCO
    // CsvWriter uses names to export header
    public class BPostData
    {
        public string PostalCode { get; set; }
        public string PostalName { get; set; }
        public bool? IsSubMunicipality { get; set; }
        public string Province { get; set; }
    }

    public static class BBostDataExtensions
    {
        public static BPostData GetRootRecord(this IList<BPostData> data)
        {
            var root = data.Count == 1
                ? data.First()
                : data.SingleOrDefault(x => (x.IsSubMunicipality.HasValue && !x.IsSubMunicipality.Value) || !x.IsSubMunicipality.HasValue);

            if (root != null)
                return root;

            if (data.Select(x => x.IsSubMunicipality).Distinct().Count() == 1 &&
                data.Select(x => x.Province).Distinct().Count() == 1)
                return data.OrderBy(x => x.PostalName).First();

            throw new InvalidOperationException("Cannot determine root record");
        }
    }
}
