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
    }
}

namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.PostalRegistry
{
    using Common;

    public class PostalInformationWasRegistered : IQueueMessage
    {
        public string PostalCode { get; }
        public Provenance Provenance { get; }

        public PostalInformationWasRegistered(
            string postalCode,
            Provenance provenance)
        {
            PostalCode = postalCode;
            Provenance = provenance;
        }
    }

    public class PostalInformationWasRealized : IQueueMessage
    {
        public string PostalCode { get; }
        public Provenance Provenance { get; }

        public PostalInformationWasRealized(
            string postalCode,
            Provenance provenance)
        {
            PostalCode = postalCode;
            Provenance = provenance;
        }
    }

    public class PostalInformationWasRetired : IQueueMessage
    {
        public string PostalCode { get; }
        public Provenance Provenance { get; }

        public PostalInformationWasRetired(
            string postalCode,
            Provenance provenance)
        {
            PostalCode = postalCode;
            Provenance = provenance;
        }
    }

    public class PostalInformationPostalNameWasAdded : IQueueMessage
    {
        public string PostalCode { get; }
        public string Language { get; }
        public string Name { get; }
        public Provenance Provenance { get; }

        public PostalInformationPostalNameWasAdded(
            string postalCode,
            string language,
            string name,
            Provenance provenance)
        {
            PostalCode = postalCode;
            Language = language;
            Name = name;
            Provenance = provenance;
        }
    }

    public class PostalInformationPostalNameWasRemoved : IQueueMessage
    {
        public string PostalCode { get; }
        public string Language { get; }
        public string Name { get; }
        public Provenance Provenance { get; }

        public PostalInformationPostalNameWasRemoved(
            string postalCode,
            string language,
            string name,
            Provenance provenance)
        {
            PostalCode = postalCode;
            Language = language;
            Name = name;
            Provenance = provenance;
        }
    }

    public class MunicipalityWasAttached : IQueueMessage
    {
        public string PostalCode { get; }
        public string NisCode { get; }
        public Provenance Provenance { get; }

        public MunicipalityWasAttached(
            string postalCode,
            string nisCode,
            Provenance provenance)
        {
            PostalCode = postalCode;
            NisCode = nisCode;
            Provenance = provenance;
        }
    }
}
