namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("MunicipalityWasLinkedToPostalInformation")]
    [EventDescription("De gemeente werd gelinked aan een postcode.")]
    public class MunicipalityWasLinkedToPostalInformation : IHasProvenance, ISetProvenance
    {
        public string PostalCode { get; }
        public string NisCode { get; }

        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasLinkedToPostalInformation(
            PostalCode postalCode,
            NisCode nisCode)
        {
            PostalCode = postalCode;
            NisCode = nisCode;
        }

        [JsonConstructor]
        private MunicipalityWasLinkedToPostalInformation(
            string postalCode,
            string nisCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode),
                new NisCode(nisCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
