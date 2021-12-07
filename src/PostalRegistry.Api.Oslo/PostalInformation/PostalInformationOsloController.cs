namespace PostalRegistry.Api.Oslo.PostalInformation
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Convertors;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("postcodes")]
    [ApiExplorerSettings(GroupName = "Postcodes")]
    public class PostalInformationOsloController : ApiController
    {
        /// <summary>
        /// Vraag info over een postcode op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="postalCode">De postcode.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de postcode gevonden is.</response>
        /// <response code="404">Als de postcode niet gevonden kan worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{postalCode}")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(PostalInformationOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalInformationOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(PostalInformationNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] string postalCode,
            CancellationToken cancellationToken = default)
        {
            var postalInformation = await context
                .PostalInformation
                .AsNoTracking()
                .Include(item => item.PostalNames)
                .SingleOrDefaultAsync(item => item.PostalCode == postalCode, cancellationToken);

            if (postalInformation == null)
                throw new ApiException("Onbestaande postcode.", StatusCodes.Status404NotFound);

            return Ok(
                new PostalInformationOsloResponse(
                    responseOptions.Value.Naamruimte,
                    postalCode,
                    postalInformation.VersionTimestamp.ToBelgianDateTimeOffset(),
                    postalInformation.IsRetired
                        ? PostInfoStatus.Gehistoreerd
                        : PostInfoStatus.Gerealiseerd)
                {
                    Postnamen = postalInformation
                        .PostalNames?
                        .Select(name => new Postnaam(new GeografischeNaam(name.Name, name.Language.ConvertFromLanguage())))
                        .ToList()
                });
        }

        /// <summary>
        /// Vraag een lijst met actieve postcodes op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="reponseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met postcodes gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(PostalInformationListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalInformationListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext legacyContext,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> reponseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalInformationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedPostalInformationSet =
                new PostalInformationListOsloQuery(legacyContext, syndicationContext)
                    .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedPostalInformationSet);

            var postalInformationSet = await pagedPostalInformationSet.Items
                .Include(x => x.PostalNames)
                .ToListAsync(cancellationToken);

            var items = postalInformationSet
                .Select(p => new PostalInformationListItemOsloResponse(
                    p.PostalCode,
                    reponseOptions.Value.Naamruimte,
                    reponseOptions.Value.DetailUrl,
                    p.IsRetired ? PostInfoStatus.Gehistoreerd : PostInfoStatus.Gerealiseerd,
                    p.VersionTimestamp.ToBelgianDateTimeOffset())
                {
                    Postnamen = p.PostalNames.Select(x => x.ConvertFromPostalName()).ToList()
                }).ToList();

            return Ok(new PostalInformationListOsloResponse
            {
                PostInfoObjecten = items,
                Volgende = BuildNextUri(pagedPostalInformationSet.PaginationInfo, reponseOptions.Value.VolgendeUrl)
            });
        }

        /// <summary>
        /// Vraag het totaal aantal van actieve postcodes op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountOsloResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalInformationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new PostalInformationListOsloQuery(context, syndicationContext)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : await context
                            .PostalInformation
                            .CountAsync(cancellationToken)
                });
        }

        private static Uri BuildNextUri(PaginationInfo paginationInfo, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
