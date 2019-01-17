namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("PostalInformationBecameCurrent")]
    [EventDescription("De postcode werd in gebruik genomen.")]
    public class PostalInformationBecameCurrent : IHasProvenance, ISetProvenance
    {
        public string PostalCode { get; }

        public ProvenanceData Provenance { get; private set; }

        public PostalInformationBecameCurrent(
            PostalCode postalCode)
        {
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private PostalInformationBecameCurrent(
            string postalCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
