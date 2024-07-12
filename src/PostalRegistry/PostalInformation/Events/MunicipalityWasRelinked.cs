namespace PostalRegistry.PostalInformation.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("MunicipalityWasRelinked")]
    [EventDescription("Het PostInfo-object werd herkoppeld aan een andere gemeente.")]
    public class MunicipalityWasRelinked : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object.")]
        public string PostalCode { get; }

        [EventPropertyDescription("NIS-code (= objectidentificator) van de nieuwe gemeente aan dewelke het PostInfo-object is toegewezen.")]
        public string NewNisCode { get; }

        [EventPropertyDescription("NIS-code (= objectidentificator) van de vorige gemeente aan dewelke het PostInfo-object was toegewezen.")]
        public string PreviousNisCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasRelinked(
            PostalCode postalCode,
            NisCode newNisCode,
            NisCode previousNisCode)
        {
            PostalCode = postalCode;
            NewNisCode = newNisCode;
            PreviousNisCode = previousNisCode;
        }

        [JsonConstructor]
        private MunicipalityWasRelinked(
            string postalCode,
            string newNisCode,
            string previousNisCode,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode),
                new NisCode(newNisCode),
                new NisCode(previousNisCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
