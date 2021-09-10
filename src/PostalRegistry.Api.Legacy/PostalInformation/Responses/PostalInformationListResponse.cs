namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PostinfoCollectie", Namespace = "")]
    public class PostalInformationListResponse
    {
        /// <summary>
        /// De verzameling van postcodes.
        /// </summary>
        [DataMember(Name = "PostInfoObjecten", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PostalInformationListItemResponse> PostInfoObjecten { get; set; }

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
    public class PostalInformationListItemResponse
    {
        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        ///  De URL die naar de details van de meest recente versie van de postcode leidt.
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

        public PostalInformationListItemResponse(
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

    public class PostalInformationListResponseExamples : IExamplesProvider<PostalInformationListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationListResponse GetExamples()
        {
            var postalInformationSampleGent =
                new PostalInformationListItemResponse("9000", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PostInfoStatus.Gerealiseerd, DateTimeOffset.Now.LocalDateTime)
                {
                    Postnamen = new List<Postnaam>
                    {
                        new Postnaam(new GeografischeNaam("Gent", Taal.NL))
                    }
                };

            var postalInformationSampleTemse =
                new PostalInformationListItemResponse("9140", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PostInfoStatus.Gerealiseerd, DateTimeOffset.Now)
                {
                    Postnamen = new List<Postnaam>
                    {
                        new Postnaam(new GeografischeNaam("Temse", Taal.NL))
                    }
                };

            return new PostalInformationListResponse
            {
                PostInfoObjecten = new List<PostalInformationListItemResponse>
                {
                    postalInformationSampleGent,
                    postalInformationSampleTemse
                },
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
