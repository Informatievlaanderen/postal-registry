namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PostinfoDetail", Namespace = "")]
    public class PostalInformationResponse
    {
        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in meerdere talen.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<Postnaam> Postnamen { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PostInfoStatus PostInfoStatus { get; set; }

        public PostalInformationResponse(
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

    public class PostalInformationResponseExamples : IExamplesProvider<PostalInformationResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public PostalInformationResponse GetExamples()
        {
            return new PostalInformationResponse(
                _responseOptions.Naamruimte,
                "9000",
                DateTimeOffset.Now,
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
        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:postal:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande postcode.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }
}
