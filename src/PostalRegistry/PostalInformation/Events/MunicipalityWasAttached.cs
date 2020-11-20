namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("MunicipalityWasAttached")]
    [EventDescription("Het PostInfo-object werd gekoppeld aan een gemeente.")]
    public class MunicipalityWasAttached : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }
        
        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente aan dewelke het PostInfo-object is toegewezen.")]
        public string NisCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasAttached(
            PostalCode postalCode,
            NisCode nisCode)
        {
            PostalCode = postalCode;
            NisCode = nisCode;
        }

        [JsonConstructor]
        private MunicipalityWasAttached(
            string postalCode,
            string nisCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode),
                new NisCode(nisCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
