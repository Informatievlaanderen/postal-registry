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
    using Exceptions;

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
                ApplyChange(new PostalInformationWasRealized(PostalCode));

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

        public void RelinkMunicipality(NisCode newNisCode)
        {
            if (newNisCode is null)
                throw new InvalidNisCodeException(newNisCode);

            if(NisCode is null)
                ApplyChange(new MunicipalityWasAttached(PostalCode, newNisCode));

            if(newNisCode == NisCode!)
                return;

            ApplyChange(new MunicipalityWasRelinked(PostalCode, newNisCode, NisCode!));
        }

        public void UpdatePostalNames(
            IReadOnlyCollection<PostalName> postalNamesToAdd,
            IReadOnlyCollection<PostalName> postalNamesToRemove)
        {
            foreach (var postalName in postalNamesToRemove.Where(_postalNames.Contains))
                ApplyChange(new PostalInformationPostalNameWasRemoved(PostalCode, postalName));

            foreach (var postalName in postalNamesToAdd)
            {
                if(_postalNames.Contains(postalName))
                   throw new PostalNameAlreadyExistsException(postalName);

                ApplyChange(new PostalInformationPostalNameWasAdded(PostalCode, postalName));
            }
        }

        public void Delete()
        {
            ApplyChange(new PostalInformationWasRemoved(PostalCode));
        }
    }
}
