namespace PostalRegistry.PostalInformation.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class UpdatePostalNames : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("6af54580-27cc-4b16-a6c9-d14eee229ed8");

        public PostalCode PostalCode { get; }

        public IReadOnlyCollection<PostalName> PostalNamesToAdd { get; }
        public IReadOnlyCollection<PostalName> PostalNamesToRemove { get; }
        public Provenance Provenance { get; }

        public UpdatePostalNames(
            PostalCode postalCode,
            IEnumerable<PostalName> postalNamesToAdd,
            IEnumerable<PostalName> postalNamesToRemove,
            Provenance provenance)
        {
            PostalCode = postalCode;
            PostalNamesToAdd = postalNamesToAdd.ToList();
            PostalNamesToRemove = postalNamesToRemove.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"UpdatePostalNames-{ToString()}");

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
