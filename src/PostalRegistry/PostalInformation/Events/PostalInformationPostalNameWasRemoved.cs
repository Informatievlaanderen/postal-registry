namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("PostalInformationPostalNameWasRemoved")]
    [EventDescription("Aan de postcode werd een postnaam verwijderd.")]
    public class PostalInformationPostalNameWasRemoved : IHasProvenance, ISetProvenance
    {
        public string PostalCode { get; }
        public string Name { get; }
        public Language Language { get; }

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
