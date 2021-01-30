namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("PostalInformationPostalNameWasAdded")]
    [EventDescription("Er werd een postnaam toegevoegd aan het PostInfo-object.")]
    public class PostalInformationPostalNameWasAdded : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Officiële spelling van de postnaam.")]
        public string Name { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarin de officiële naam staat.")]
        public Language Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationPostalNameWasAdded(
            PostalCode postalCode,
            PostalName postalName)
        {
            PostalCode = postalCode;
            Name = postalName.Name;
            Language = postalName.Language;
        }

        [JsonConstructor]
        private PostalInformationPostalNameWasAdded(
            string postalCode,
            string name,
            Language language,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode),
                new PostalName(name, language)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
