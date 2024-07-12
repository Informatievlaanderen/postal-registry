namespace PostalRegistry.PostalInformation.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RelinkMunicipality : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("461f480c-75d9-4637-b98e-5fd66682d338");

        public PostalCode PostalCode { get; }
        public NisCode NewNisCode { get; }
        public Provenance Provenance { get; }

        public RelinkMunicipality(PostalCode postalCode, NisCode newNisCode, Provenance provenance)
        {
            PostalCode = postalCode;
            NewNisCode = newNisCode;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RelinkMunicipality-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PostalCode;
            yield return NewNisCode;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
