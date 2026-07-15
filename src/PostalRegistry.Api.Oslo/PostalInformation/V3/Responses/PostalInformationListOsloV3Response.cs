namespace PostalRegistry.Api.Oslo.PostalInformation.V3.Responses
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.PostInfo;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class PostalInformationListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de postinfo.
        /// </summary>
        [JsonProperty("@context", Required = Required.DisallowNull, Order = 0)]
        public required string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de postinfo.
        /// </summary>
        [JsonProperty( "@type", Required = Required.DisallowNull, Order = 1)]
        public string Type => "PostinfoLijstEnvelop";

        /// <summary>
        /// De verzameling van postcodes.
        /// </summary>
        [JsonProperty("data", Required = Required.DisallowNull, Order = 2)]
        public required List<PostalInformationListItemOsloV3Response> PostInfoObjecten { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [JsonProperty("volgende", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public Uri? Volgende { get; set; }
    }

    public class PostalInformationListItemOsloV3Response
    {
        /// <summary>
        /// Het linked-data type van de postinfo.
        /// </summary>
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 0)]
        public string Type => "PostInfo";

        /// <summary>
        /// De unieke en persistente identificator van de postinfo (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty("@id", Required = Required.DisallowNull, Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [JsonProperty("identificator", Required = Required.DisallowNull, Order = 2)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        ///  De URL die de details van de meest recente versie van de postinfo weergeeft.
        /// </summary>
        [JsonProperty("detail", Required = Required.DisallowNull, Order = 3)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [JsonProperty("status", Required = Required.DisallowNull, Order = 4)]
        public Status PostInfoStatus { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [JsonProperty("postnaam", Required = Required.DisallowNull, Order = 5)]
        public List<GeografischeNaam> Postnamen { get; set; }

        public PostalInformationListItemOsloV3Response(
            string postalCode,
            string detail,
            PostInfoStatus status,
            IEnumerable<GeografischeNaam> postnamen,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Postinfo.ToPuri(postalCode);
            Identificator = new PostinfoIdentificator(postalCode, version);
            Detail = new Uri(string.Format(detail, postalCode));
            PostInfoStatus = new Status(status);
            Postnamen = new List<GeografischeNaam>(postnamen);
        }
    }

    public class PostalInformationListOsloResponseExamples : IExamplesProvider<PostalInformationListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public PostalInformationListOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationListOsloV3Response GetExamples()
        {
            var postalInformationSampleGent =
                new PostalInformationListItemOsloV3Response(
                    "9000",
                    _responseOptions.DetailUrl,
                    PostInfoStatus.Gerealiseerd,
                    [new GeografischeNaam("Gent", Taal.Nl)],
                    DateTimeOffset.Now.ToExampleOffset());

            var postalInformationSampleTemse =
                new PostalInformationListItemOsloV3Response(
                    "9140",
                    _responseOptions.DetailUrl,
                    PostInfoStatus.Gerealiseerd,
                    [new GeografischeNaam("Temse", Taal.Nl)],
                    DateTimeOffset.Now.ToExampleOffset());

            return new PostalInformationListOsloV3Response
            {
                PostInfoObjecten = new List<PostalInformationListItemOsloV3Response>
                {
                    postalInformationSampleGent,
                    postalInformationSampleTemse
                },
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
