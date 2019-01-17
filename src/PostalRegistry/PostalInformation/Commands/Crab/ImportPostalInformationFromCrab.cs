namespace PostalRegistry.PostalInformation.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportPostalInformationFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("3f5908cf-a267-4358-b2b4-57ffa74a08c2");

        public PostalCode PostalCode { get; }
        public CrabSubCantonId SubCantonId { get; }
        public CrabSubCantonCode SubCantonCode { get; }
        public NisCode NisCode { get; }

        public CrabMunicipalityName MunicipalityName { get; }

        public CrabLifetime Lifetime { get; }

        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportPostalInformationFromCrab(
            PostalCode postalCode,
            CrabSubCantonId subCantonId,
            CrabSubCantonCode subCantonCode,
            NisCode nisCode,
            CrabMunicipalityName municipalityName,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            PostalCode = postalCode;
            SubCantonId = subCantonId;
            SubCantonCode = subCantonCode;
            NisCode = nisCode;

            MunicipalityName = municipalityName;

            Lifetime = lifetime;

            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportPostalInformationFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return PostalCode;
            yield return SubCantonId;
            yield return SubCantonCode;
            yield return NisCode;
            yield return MunicipalityName;
            yield return Lifetime;
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
