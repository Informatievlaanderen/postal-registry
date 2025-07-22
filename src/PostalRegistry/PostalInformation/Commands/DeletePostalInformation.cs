namespace PostalRegistry.PostalInformation.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class DeletePostalInformation : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("1c7c84c9-9c16-41e0-9c34-096b24d775d6");

        public PostalCode PostalCode { get; }
        public Provenance Provenance { get; }

        public DeletePostalInformation(PostalCode postalCode, Provenance provenance)
        {
            PostalCode = postalCode;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DeletePostalInformation-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PostalCode;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
