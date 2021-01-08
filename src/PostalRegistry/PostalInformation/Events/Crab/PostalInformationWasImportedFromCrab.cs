namespace PostalRegistry.PostalInformation.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("Crab-PostalInformationWasImported")]
    [EventDescription("Er werd postinformatie geïmporteerd uit CRAB.")]
    public class PostalInformationWasImportedFromCrab
    {
        [EventPropertyDescription("Subkantonidentificator. Postkantons die gemeentegrenzen overschrijden worden onderverdeeld in subkantons.")]
        public int SubCantonId { get; }
        
        [EventPropertyDescription("Subkantonnummer.")]
        public string SubCantonCode { get; }
        
        [EventPropertyDescription("Postcode.")]
        public string PostalCode { get; }
        
        [EventPropertyDescription("NIS-code van de gemeente waaraan de postcode is gekoppeld.")]
        public string NisCode { get; }

        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDate { get; }

        [EventPropertyDescription("Officiële spelling van de gemeente.")]
        public string MunicipalityName { get; }
        
        [EventPropertyDescription("Taal waarin de officiële naam staat.")]
        public CrabLanguage? MunicipalityNameLanguage { get; }

        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")] 
        public Instant Timestamp { get; }

        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }
        
        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")] 
        public CrabModification? Modification { get; }
        
        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
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
                  organisation) { }
    }
}
