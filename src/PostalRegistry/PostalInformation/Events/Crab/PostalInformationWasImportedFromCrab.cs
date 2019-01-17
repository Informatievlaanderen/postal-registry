namespace PostalRegistry.PostalInformation.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("Crab-PostalInformationWasImported")]
    [EventDescription("De postcode werd geïmporteerd van CRAB.")]
    public class PostalInformationWasImportedFromCrab
    {
        public int SubCantonId { get; }
        public string SubCantonCode { get; }
        public string PostalCode { get; }
        public string NisCode { get; }

        public LocalDateTime? BeginDate { get; }

        public string MunicipalityName { get; }
        public CrabLanguage? MunicipalityNameLanguage { get; }

        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public PostalInformationWasImportedFromCrab(
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

            BeginDate = lifetime.BeginDateTime;

            MunicipalityName = municipalityName.Name;
            MunicipalityNameLanguage = municipalityName.Language;

            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private PostalInformationWasImportedFromCrab(
            string postalCode,
            int subCantonId,
            string subCantonCode,
            string nisCode,
            string municipalityName,
            CrabLanguage? municipalityNameLanguage,
            LocalDateTime? beginDate,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
            : this(
                  new PostalCode(postalCode),
                  new CrabSubCantonId(subCantonId),
                  new CrabSubCantonCode(subCantonCode),
                  new NisCode(nisCode),
                  new CrabMunicipalityName(municipalityName, municipalityNameLanguage),
                  new CrabLifetime(beginDate, null),
                  new CrabTimestamp(timestamp),
                  new CrabOperator(@operator),
                  modification,
                  organisation)
        { }
    }
}
