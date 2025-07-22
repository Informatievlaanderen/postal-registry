namespace PostalRegistry.Producer.Ldes
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PostalInformationLdes
    {
        private static readonly JObject Context = JObject.Parse(@"
{
    ""@version"": 1.1,
    ""@base"": ""https://basisregisters.vlaanderen.be/implementatiemodel/adressenregister"",
    ""@vocab"": ""#"",
    ""identificator"": ""@nest"",
    ""id"": ""@id"",
    ""versieId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""naamruimte"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#naamruimte"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""objectId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#lokaleIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""postInfoStatus"": {
      ""@id"": ""https://basisregisters.vlaanderen.be/implementatiemodel/adressenregister#Postinfo%3Astatus"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/doc/concept/postinfostatus/""
      }
    },
    ""gemeente"": {
      ""@id"": ""http://www.w3.org/ns/prov#wasAttributedTo"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/gemeente/"",
        ""objectId"": ""@id""
      }
    },
    ""postnamen"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#postnaam"",
      ""@container"": ""@language""
    }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "PostInfo";

        [JsonProperty("Identificator", Order = 2)]
        public PostinfoIdentificator Identificator { get; private set; }

        [JsonProperty("Gemeente", Order = 3)]
        public GemeenteObjectId? Gemeente { get; private set; }

        [JsonProperty("Postnamen", Order = 4)]
        public Dictionary<string, ICollection<string>> Postnamen { get; private set; }

        [JsonProperty("PostInfoStatus", Order = 5)]
        public PostInfoStatus PostInfoStatus { get; private set; }

        [JsonProperty("isVerwijderd", Order = 6)]
        public bool IsRemoved { get; private set; }

        public PostalInformationLdes(PostalInformationDetail postalInformation, string osloNamespace)
        {
            Identificator = new PostinfoIdentificator(osloNamespace, postalInformation.PostalCode, postalInformation.VersionTimestamp.ToBelgianDateTimeOffset());
            Gemeente = postalInformation.NisCode is not null
                ? new GemeenteObjectId(postalInformation.NisCode)
                : null;
            Postnamen = new Dictionary<string, ICollection<string>>(
                new[]
                    {
                        ("nl", postalInformation.NamesDutch),
                        ("fr", postalInformation.NamesFrench),
                        ("de", postalInformation.NamesGerman),
                        ("en", postalInformation.NamesEnglish)
                    }
                    .Where(pair => pair.Item2.Count > 0)
                    .ToDictionary(pair => pair.Item1, pair => pair.Item2)
            );
            PostInfoStatus = postalInformation.IsRetired
                ? PostInfoStatus.Gehistoreerd
                : PostInfoStatus.Gerealiseerd;
            IsRemoved = postalInformation.IsRemoved;
        }
    }

    public sealed class GemeenteObjectId
    {
        [JsonProperty("ObjectId")] public string ObjectId { get; private set; }

        public GemeenteObjectId(string objectId)
        {
            ObjectId = objectId;
        }
    }
}
