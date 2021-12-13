namespace PostalRegistry.Api.Oslo.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PostinfoCollectie", Namespace = "")]
    public class PostalInformationListOsloResponse
    {
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context => @"{
    ""@base"": ""https://basisregisters.vlaanderen.be/ns/adres"",
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
    ""PostInfo"": ""https://data.vlaanderen.be/ns/adres#Postinfo"",
    ""postnamen"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#postnaam"",
      ""@type"": ""@id"",
      ""@context"": {
        ""geografischeNaam"": {
          ""@id"": ""http://www.w3.org/2000/01/rdf-schema#label"",
          ""@context"": {
            ""spelling"": ""@value"",
            ""taal"": ""@language""
          }
        }
      }
    },
    ""postInfoStatus"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#Postinfo.status"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/concept/postinfostatus/""
      }
    },
    ""detail"": ""http://www.iana.org/assignments/relation/self"",
    ""postInfoObjecten"": ""@graph""
  }";

        /// <summary>
        /// De verzameling van postcodes.
        /// </summary>
        [DataMember(Name = "PostInfoObjecten", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PostalInformationListItemOsloResponse> PostInfoObjecten { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 2)]
        //[JsonProperty(Required = Required.DisallowNull)]
        //public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "PostInfo", Namespace = "")]
    public class PostalInformationListItemOsloResponse
    {
        /// <summary>
        /// Het linked-data type van de postcode.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "PostInfo";

        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        ///  De URL die de details van de meest recente versie van de postinfo weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostInfoStatus PostInfoStatus { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<Postnaam> Postnamen { get; set; }

        public PostalInformationListItemOsloResponse(
            string postalCode,
            string naamruimte,
            string detail,
            PostInfoStatus status,
            DateTimeOffset? version)
        {
            Identificator = new PostinfoIdentificator(naamruimte, postalCode, version);
            Detail = new Uri(string.Format(detail, postalCode));
            PostInfoStatus = status;
        }
    }

    public class PostalInformationListOsloResponseExamples : IExamplesProvider<PostalInformationListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationListOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationListOsloResponse GetExamples()
        {
            var postalInformationSampleGent =
                new PostalInformationListItemOsloResponse("9000", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PostInfoStatus.Gerealiseerd, DateTimeOffset.Now.ToExampleOffset())
                {
                    Postnamen = new List<Postnaam>
                    {
                        new Postnaam(new GeografischeNaam("Gent", Taal.NL))
                    }
                };

            var postalInformationSampleTemse =
                new PostalInformationListItemOsloResponse("9140", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PostInfoStatus.Gerealiseerd, DateTimeOffset.Now.ToExampleOffset())
                {
                    Postnamen = new List<Postnaam>
                    {
                        new Postnaam(new GeografischeNaam("Temse", Taal.NL))
                    }
                };

            return new PostalInformationListOsloResponse
            {
                PostInfoObjecten = new List<PostalInformationListItemOsloResponse>
                {
                    postalInformationSampleGent,
                    postalInformationSampleTemse
                },
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
