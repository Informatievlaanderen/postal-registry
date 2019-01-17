namespace PostalRegistry.PostalInformation.Commands.BPost
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportPostalInformationFromBPost : IHasBPostProvenance
    {
        private static readonly Guid Namespace = new Guid("870805cd-9bd9-4f35-bec0-5cf1517e8199");

        public PostalCode PostalCode { get; }
        public List<PostalName> PostalNames { get; }
        public bool? IsSubMunicipality { get; }

        public Province Province { get; }

        public BPostTimestamp Timestamp { get; }
        public Modification Modification { get; }

        public ImportPostalInformationFromBPost(
            PostalCode postalCode,
            List<PostalName> postalNames,
            bool? isSubMunicipality,
            Province province,
            BPostTimestamp timestamp,
            Modification modification)
        {
            PostalCode = postalCode;
            PostalNames = postalNames;
            IsSubMunicipality = isSubMunicipality;

            Province = province;

            Timestamp = timestamp;
            Modification = modification;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportPostalInformationFromBPost-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return PostalCode;
            yield return PostalNames;
            yield return IsSubMunicipality;
            yield return Province;
            yield return Timestamp;
            yield return Modification;
        }
    }
}
