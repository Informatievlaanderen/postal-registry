namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("PostalInformationWasRemoved")]
    [EventDescription("Het PostInfo-object werd verwijderd uit het register.")]
    public class PostalInformationWasRemoved : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationWasRemoved(PostalCode postalCode)
        {
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private PostalInformationWasRemoved(string postalCode, ProvenanceData provenance) :
            this(new PostalCode(postalCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
