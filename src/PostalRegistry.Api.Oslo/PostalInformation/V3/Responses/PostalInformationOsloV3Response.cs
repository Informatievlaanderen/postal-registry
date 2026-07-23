namespace PostalRegistry.Api.Oslo.PostalInformation.V3.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.PostInfo;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;
    using Status = Be.Vlaanderen.Basisregisters.GrAr.Oslo.PostInfo.PostInfoStatus;

    public class PostalInformationOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de postinfo.
        /// </summary>
        [JsonProperty("@context", Required = Required.DisallowNull, Order = 0)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de postinfo envelop.
        /// </summary>
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 1)]
        public string Type => "PostinfoEnvelop";

        /// <summary>
        /// De data van de postinfo.
        /// </summary>
        [JsonProperty("data", Required = Required.DisallowNull, Order = 2)]
        public PostalInformationOsloV3ResponseData Data { get; set; }

        /// <summary>
        /// De hyperlinks die gerelateerd zijn aan de postinfo.
        /// </summary>
        [DataMember(Name = "_links", Order = 99)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PostalInformationOsloV3ResponseLinks? Links { get; set; }

        public PostalInformationOsloV3Response(
            string contextUrlDetail,
            string postcode,
            PostinfoToegekendAanGemeente? gemeente,
            IEnumerable<GeografischeNaam> postnamen,
            DateTimeOffset version,
            PostInfoStatusValue postInfoStatus,
            string? nuts3Code,
            string selfDetailUrl,
            string addressesLinkUrl)
        {
            Context = contextUrlDetail;
            Data = new PostalInformationOsloV3ResponseData(postcode, gemeente, postnamen, version, postInfoStatus, nuts3Code);

            Links = new PostalInformationOsloV3ResponseLinks(
                self: new Link
                {
                    Href = new Uri(string.Format(selfDetailUrl, postcode))
                },
                adressen: new Link
                {
                    Href = new Uri(string.Format(addressesLinkUrl, postcode))
                }
            );
        }
    }

    /// <summary>
    /// De data van de postinfo.
    /// </summary>
    public class PostalInformationOsloV3ResponseData
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
        /// De gemeente aan dewelke de postinfo is toegewezen.
        /// </summary>
        [JsonProperty("isToegekendAan", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public PostinfoToegekendAanGemeente? Gemeente { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [JsonProperty("postnaam", Required = Required.DisallowNull, Order = 4)]
        public List<GeografischeNaam> Postnamen { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [JsonProperty("status", Required = Required.DisallowNull, Order = 5)]
        public Status PostInfoStatus { get; set; }

        /// <summary>
        /// De NUTS3 classificatie gebruikt door Eurostat.
        /// </summary>
        [JsonProperty("nuts3", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public string? Nuts3Code { get; set; }

        public PostalInformationOsloV3ResponseData(
            string postcode,
            PostinfoToegekendAanGemeente? gemeente,
            IEnumerable<GeografischeNaam> postnamen,
            DateTimeOffset version,
            PostInfoStatusValue postInfoStatus,
            string? nuts3Code)
        {
            Id = OsloNamespaces.Postinfo.ToPuri(postcode);
            Identificator = new PostinfoIdentificator(postcode, version);
            Gemeente = gemeente;
            PostInfoStatus = new PostInfoStatus(postInfoStatus);
            Postnamen = new List<GeografischeNaam>(postnamen);
            Nuts3Code = nuts3Code;
        }
    }

    /// <summary>
    /// De hyperlinks die gerelateerd zijn aan de postinfo.
    /// </summary>
    public class PostalInformationOsloV3ResponseLinks
    {
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 0)]
        public string Type => "Links";

        [JsonProperty("self", Required = Required.DisallowNull, Order = 1)]
        public Link Self { get; set; }

        [JsonProperty("adressen", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public Link? Adressen { get; set; }

        public PostalInformationOsloV3ResponseLinks(
            Link self,
            Link? adressen = null)
        {
            Self = self;
            Adressen = adressen;
        }
    }

    public class PostalInformationOsloResponseExamples : IExamplesProvider<PostalInformationOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public PostalInformationOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationOsloV3Response GetExamples()
        {
            var gemeente = new PostinfoToegekendAanGemeente
            {
                Id = OsloNamespaces.Gemeente.ToPuri("31005"),
                Detail = string.Format(_responseOptions.GemeenteDetailUrl, "31005"),
                Gemeentenaam = new Gemeentenaam
                {
                    Gemeentenamen = [new GeografischeNaam("Brugge", Taal.Nl)]
                }
            };

            return new PostalInformationOsloV3Response(
                _responseOptions.ContextUrlDetail,
                "8200",
                gemeente,
                [new GeografischeNaam("Sint-Andries", Taal.Nl), new GeografischeNaam("Sint-Michiels", Taal.Nl)],
                DateTimeOffset.Now.ToExampleOffset(),
                PostInfoStatusValue.Gerealiseerd,
                "BE251",
                _responseOptions.DetailUrl,
                _responseOptions.PostInfoDetailAddressesLink);
        }
    }

    public class PostalInformationNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public PostalInformationNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:postalcode:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande postcode.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }

    public class PostalInformationGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public PostalInformationGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:postalcode:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderde postcode.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }
}
