namespace PostalRegistry.PostalInformation.Events.BPost
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures.BPost;
    using Newtonsoft.Json;

    [EventName("BPost-PostalInformationWasImported")]
    [EventDescription("Er werd postinformatie geïmporteerd van bpost.")]
    public class PostalInformationWasImportedFromBPost : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Postcode.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Postnamen die bij de postcode horen.")]
        public List<PostalNameData> PostalNames { get; }

        [EventPropertyDescription("Aanduiding of de postcode naar een deelgemeente of andere postbedelingszone (binnen een gemeente) verwijst.")]
        public bool? IsSubMunicipality { get; }

        [EventPropertyDescription("Provincie waarbinnen de postcode gebruikt wordt.")]
        public string Province { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public PostalInformationWasImportedFromBPost(
            PostalCode postalCode,
            List<PostalName> postalNames,
            bool? isSubMunicipality,
            Province province)
        {
            PostalCode = postalCode;
            IsSubMunicipality = isSubMunicipality;
            PostalNames = postalNames?.Select(x => new PostalNameData(x.Name, x.Language)).ToList();
            Province = province;
        }

        [JsonConstructor]
        private PostalInformationWasImportedFromBPost(
            string postalCode,
            List<PostalNameData> postalNames,
            bool? isSubMunicipality,
            string province,
            ProvenanceData provenance) :
            this(
                new PostalCode(postalCode),
                postalNames?.Select(x => new PostalName(x.Name, x.Language)).ToList(),
                isSubMunicipality,
                new Province(province)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
