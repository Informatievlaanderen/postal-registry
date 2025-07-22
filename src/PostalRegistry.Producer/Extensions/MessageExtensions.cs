namespace PostalRegistry.Producer.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.PostalRegistry;
    using ContractsCommon = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Domain = PostalInformation.Events;

    public static class MessageExtensions
    {
        private static ContractsCommon.Provenance ToContract(this ProvenanceData provenance) => new ContractsCommon.Provenance(
            provenance.Timestamp.ToString(),
            provenance.Application.ToString(),
            provenance.Modification.ToString(),
            provenance.Organisation.ToString(),
            provenance.Reason);

        public static Contracts.PostalInformationWasRegistered ToContract(this Domain.PostalInformationWasRegistered message) =>
            new Contracts.PostalInformationWasRegistered(message.PostalCode, message.Provenance.ToContract());

        public static Contracts.PostalInformationWasRealized ToContract(this Domain.PostalInformationWasRealized message) =>
            new Contracts.PostalInformationWasRealized(message.PostalCode, message.Provenance.ToContract());

        public static Contracts.PostalInformationWasRetired ToContract(this Domain.PostalInformationWasRetired message) =>
            new Contracts.PostalInformationWasRetired(message.PostalCode, message.Provenance.ToContract());

        public static Contracts.PostalInformationPostalNameWasAdded ToContract(this Domain.PostalInformationPostalNameWasAdded message) =>
            new Contracts.PostalInformationPostalNameWasAdded(message.PostalCode, message.Language.ToString(), message.Name, message.Provenance.ToContract());

        public static Contracts.PostalInformationPostalNameWasRemoved ToContract(this Domain.PostalInformationPostalNameWasRemoved message) =>
            new Contracts.PostalInformationPostalNameWasRemoved(message.PostalCode, message.Language.ToString(), message.Name, message.Provenance.ToContract());

        public static Contracts.MunicipalityWasAttached ToContract(this Domain.MunicipalityWasAttached message) =>
            new Contracts.MunicipalityWasAttached(message.PostalCode, message.NisCode, message.Provenance.ToContract());

        public static Contracts.MunicipalityWasRelinked ToContract(this Domain.MunicipalityWasRelinked message) =>
            new Contracts.MunicipalityWasRelinked(message.PostalCode, message.NewNisCode, message.PreviousNisCode, message.Provenance.ToContract());

        public static Contracts.PostalInformationWasRemoved ToContract(this Domain.PostalInformationWasRemoved message) =>
            new Contracts.PostalInformationWasRemoved(message.PostalCode, message.Provenance.ToContract());
    }
}
