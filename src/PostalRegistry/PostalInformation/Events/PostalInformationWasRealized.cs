namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("PostalInformationWasRealized")]
    [EventDescription("Het PostInfo-object kreeg status 'gerealiseerd'.")]
    public class PostalInformationWasRealized : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationWasRealized(
            PostalCode postalCode)
        {
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private PostalInformationWasRealized(
            string postalCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
