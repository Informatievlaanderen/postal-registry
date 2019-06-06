namespace PostalRegistry.PostalInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.BPost;
    using Events.Crab;

    public partial class PostalInformation : AggregateRootEntity
    {
        public static readonly Func<PostalInformation> Factory = () => new PostalInformation();

        public static PostalInformation Register(PostalCode id)
        {
            var postalInformation = Factory();
            postalInformation.ApplyChange(new PostalInformationWasRegistered(id));
            return postalInformation;
        }

        public void ImportPostalInformationFromBPost(
            PostalCode postalCode,
            List<PostalName> postalNames,
            bool? isSubMunicipality,
            Province province,
            Modification modification)
        {
            if (modification == Modification.Delete && _status != PostalInformationStatus.Retired)
                ApplyChange(new PostalInformationWasRetired(PostalCode));

            if (modification != Modification.Delete && _status != PostalInformationStatus.Retired)
                ApplyChange(new PostalInformationBecameCurrent(PostalCode));

            foreach (var postalName in postalNames.Where(x => !_postalNames.Contains(x)))
                ApplyChange(new PostalInformationPostalNameWasAdded(PostalCode, postalName));

            var namesToRemove = _postalNames.Where(x => !postalNames.Contains(x)).ToList();
            foreach (var postalName in namesToRemove)
                ApplyChange(new PostalInformationPostalNameWasRemoved(PostalCode, postalName));

            ApplyChange(new PostalInformationWasImportedFromBPost(
                postalCode,
                postalNames,
                isSubMunicipality,
                province));
        }

        public void ImportPostalInformationFromCrab(
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
            ApplyChange(
                new MunicipalityWasAttached(
                    new PostalCode(subCantonCode),
                    nisCode));

            ApplyChange(
                new PostalInformationWasImportedFromCrab(
                    postalCode,
                    subCantonId,
                    subCantonCode,
                    nisCode,
                    municipalityName,
                    lifetime,
                    timestamp,
                    @operator,
                    modification,
                    organisation));
        }
    }
}
