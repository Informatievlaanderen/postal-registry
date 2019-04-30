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
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PostinfoDetail", Namespace = "")]
    public class PostalInformationResponse
    {
        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in meerdere talen.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 2)]
        public List<Postnaam> Postnamen { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 3)]
        public PostInfoStatus PostInfoStatus { get; set; }

        public PostalInformationResponse(
            string naamruimte,
            string postcode,
            DateTimeOffset version,
            PostInfoStatus postInfoStatus)
        {
            Identificator = new Identificator(naamruimte, postcode, version);
            PostInfoStatus = postInfoStatus;
            Postnamen = new List<Postnaam>();
        }
    }

    public class PostalInformationResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
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

    public class PostalInformationNotFoundResponseExamples : IExamplesProvider
    {
        public object GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande postcode.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }
}
