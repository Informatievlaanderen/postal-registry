namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("PostalInformationWasRegistered")]
    [EventDescription("Het postinfo-object werd aangemaakt in het register met zijn persistente lokale identificator.")]
    public class PostalInformationWasRegistered : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationWasRegistered(
            PostalCode postalCode)
        {
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private PostalInformationWasRegistered(
            string postalCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
