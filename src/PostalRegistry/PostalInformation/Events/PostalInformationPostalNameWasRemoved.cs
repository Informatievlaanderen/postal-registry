namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("PostalInformationPostalNameWasRemoved")]
    [EventDescription("Er werd een postnaam verwijderd uit het PostInfo-object.")]
    public class PostalInformationPostalNameWasRemoved : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Officiële spelling van de postnaam.")]
        public string Name { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarin de officiële naam staat.")]
        public Language Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationPostalNameWasRemoved(
            PostalCode postalCode,
            PostalName postalName)
        {
            PostalCode = postalCode;
            Name = postalName.Name;
            Language = postalName.Language;
        }

        [JsonConstructor]
        private PostalInformationPostalNameWasRemoved(
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
