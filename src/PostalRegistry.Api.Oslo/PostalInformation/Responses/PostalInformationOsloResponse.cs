namespace PostalRegistry.Api.Oslo.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
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
    ""postInfoStatus"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#Postinfo.status"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/concept/postinfostatus/""
      }
    },
    ""postcode"": ""https://data.vlaanderen.be/ns/adres#postcode"",
    ""postnamen"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#postnaam"",
      ""@context"": {
        ""geografischeNaam"": {
          ""@id"": ""http://www.w3.org/2000/01/rdf-schema#label"",
          ""@context"": {
            ""spelling"": ""@value"",
            ""taal"": ""@language""
          }
        }
      }
    }
  }";

        /// <summary>
        /// Het linked-data type van de postcode.
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
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<Postnaam> Postnamen { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostInfoStatus PostInfoStatus { get; set; }

        public PostalInformationOsloResponse(
            string naamruimte,
            string postcode,
            DateTimeOffset version,
            PostInfoStatus postInfoStatus)
        {
            Identificator = new PostinfoIdentificator(naamruimte, postcode, version);
            PostInfoStatus = postInfoStatus;
            Postnamen = new List<Postnaam>();
        }
    }

    public class PostalInformationOsloResponseExamples : IExamplesProvider<PostalInformationOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationOsloResponse GetExamples()
        {
            return new PostalInformationOsloResponse(
                _responseOptions.Naamruimte,
                "9000",
                DateTimeOffset.Now.ToExampleOffset(),
                PostInfoStatus.Gerealiseerd)
            {
                Postnamen = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam("Gent", Taal.NL))
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
    }
}
