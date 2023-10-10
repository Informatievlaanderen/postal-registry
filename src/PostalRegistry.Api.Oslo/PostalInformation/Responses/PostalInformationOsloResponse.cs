namespace PostalRegistry.Api.Oslo.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PostinfoDetail", Namespace = "")]
    public class PostalInformationOsloResponse
    {
        /// <summary>
        /// De linked-data context van de postinfo.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de postinfo.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "PostInfo";

        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeente aan dewelke de postinfo is toegewezen.
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PostinfoDetailGemeente? Gemeente { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<Postnaam> Postnamen { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostInfoStatus PostInfoStatus { get; set; }

        /// <summary>
        /// De NUTS3 classificatie gebruikt door Eurostat.
        /// </summary>
        [DataMember(Name = "Nuts3", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Nuts3Code { get; set; }

        public PostalInformationOsloResponse(
            string naamruimte,
            string contextUrlDetail,
            string postcode,
            PostinfoDetailGemeente? gemeente,
            DateTimeOffset version,
            PostInfoStatus postInfoStatus,
            string? nuts3Code)
        {
            Context = contextUrlDetail;
            Identificator = new PostinfoIdentificator(naamruimte, postcode, version);
            Gemeente = gemeente;
            PostInfoStatus = postInfoStatus;
            Postnamen = new List<Postnaam>();
            Nuts3Code = nuts3Code;
        }
    }

    public class PostalInformationOsloResponseExamples : IExamplesProvider<PostalInformationOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationOsloResponse GetExamples()
        {
            var gemeente = new PostinfoDetailGemeente
            {
                ObjectId = "31005",
                Detail = string.Format(_responseOptions.GemeenteDetailUrl, "31005"),
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Brugge", Taal.NL))
            };

            return new PostalInformationOsloResponse(
                _responseOptions.Naamruimte,
                _responseOptions.ContextUrlDetail,
                "8200",
                gemeente,
                DateTimeOffset.Now.ToExampleOffset(),
                PostInfoStatus.Gerealiseerd,
                "BE251")
            {
                Postnamen = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam("Sint-Andries", Taal.NL)),
                    new Postnaam(new GeografischeNaam("Sint-Michiels", Taal.NL))
                }
            };
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v2")
            };
    }
}
