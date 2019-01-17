namespace PostalRegistry.PostalInformation.Events.BPost
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures.BPost;
    using Newtonsoft.Json;

    [EventName("BPost-PostalInformationWasImported")]
    [EventDescription("De postcode werd geïmporteerd van bpost.")]
    public class PostalInformationWasImportedFromBPost : IHasProvenance, ISetProvenance
    {
        public string PostalCode { get; }
        public List<PostalNameData> PostalNames { get; }
        public bool? IsSubMunicipality { get; }

        public string Province { get; }

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
