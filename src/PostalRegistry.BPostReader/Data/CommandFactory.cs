namespace PostalRegistry.BPostReader.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using PostalInformation.Commands.BPost;

    public class CommandFactory
    {
        private readonly IEnumerable<PostalName> _postalNames;

        public CommandFactory(IEnumerable<PostalName> postalNames) => _postalNames = postalNames;

        public ImportPostalInformationFromBPost Create(
            List<BPostData> bpostData,
            BPostTimestamp timestamp,
            Modification modification)
        {
            if (bpostData.Select(x => x.PostalCode).Distinct().Count() > 1)
                throw new InvalidOperationException("Cannot create command for more than one postal code");

            var root = bpostData.GetRootRecord();

            return new ImportPostalInformationFromBPost(
                new PostalCode(root.PostalCode),
                bpostData.Select(x =>
                {
                    var lang = _postalNames
                        .Where(p => string.Equals(p.Name.Trim(), x.PostalName.Trim(), StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Language)
                        .Distinct()
                        .Single();

                    return new PostalName(x.PostalName, lang);
                }).ToList(),
                root.IsSubMunicipality,
                new Province(root.Province),
                timestamp,
                modification);
        }
    }
}
