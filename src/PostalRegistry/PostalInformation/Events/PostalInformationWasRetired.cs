namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("PostalInformationWasRetired")]
    [EventDescription("De postcode werd gehistoreerd.")]
    public class PostalInformationWasRetired : IHasProvenance, ISetProvenance
    {
        public string PostalCode { get; }
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationWasRetired(
            PostalCode postalCode)
        {
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private PostalInformationWasRetired(
            string postalCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
