namespace PostalRegistry.Api.Oslo.PostalInformation.V3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.PostInfo;
    using CloudNative.CloudEvents;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OutputCaching;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Nuts;
    using Projections.Feed;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("3.0")]
    [AdvertiseApiVersions("2.0", "3.0")]
    [ApiRoute("postcodes")]
    [ApiExplorerSettings(GroupName = "Postcodes")]
    public class PostalInformationOsloController : ApiController
    {
        /// <summary>
        /// Vraag info over een postcode op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="nuts3Service"></param>
        /// <param name="postalCode">De postcode.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de postcode gevonden is.</response>
        /// <response code="404">Als de postcode niet gevonden kan worden.</response>
        /// <response code="410">Als de postcode verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{postalCode}")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(PostalInformationOsloV3Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalInformationOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(PostalInformationNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(PostalInformationGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptionsV3> responseOptions,
            [FromServices] Nuts3Service nuts3Service,
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

            if (postalInformation.IsRemoved)
                throw new ApiException("Verwijderde postcode.", StatusCodes.Status410Gone);

            var gemeente = await GetPostinfoDetailGemeente(
                syndicationContext,
                postalInformation.NisCode,
                responseOptions.Value.GemeenteDetailUrl,
                cancellationToken);

            var nuts3Record = nuts3Service.GetNuts3ByPostalCode(postalInformation.PostalCode);

            return Ok(
                new PostalInformationOsloV3Response(
                    responseOptions.Value.ContextUrlDetail,
                    postalCode,
                    gemeente,
                    postalInformation
                        .PostalNames
                        .Select(name => new GeografischeNaam(name.Name, name.Language.ConvertOsloFromLanguage()))
                        .ToList(),
                    postalInformation.VersionTimestamp.ToBelgianDateTimeOffset(),
                    postalInformation.IsRetired
                        ? PostInfoStatusValue.Gehistoreerd
                        : PostInfoStatusValue.Gerealiseerd,
                    nuts3Record?.Nuts3Code,
                    responseOptions.Value.DetailUrl,
                    responseOptions.Value.PostInfoDetailAddressesLink));
        }

        /// <summary>
        /// Vraag een lijst met actieve postcodes op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="nuts3Service"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met postcodes gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(PostalInformationListOsloV3Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalInformationListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext legacyContext,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptionsV3> responseOptions,
            [FromServices] Nuts3Service nuts3Service,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalInformationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedPostalInformationSet =
                new PostalInformationListOsloQuery(legacyContext, syndicationContext, nuts3Service)
                    .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedPostalInformationSet);

            var postalInformationSet = await pagedPostalInformationSet.Items
                .Include(x => x.PostalNames)
                .ToListAsync(cancellationToken);

            var items = postalInformationSet
                .Select(p => new PostalInformationListItemOsloV3Response(
                    p.PostalCode,
                    responseOptions.Value.DetailUrl,
                    p.IsRetired ? PostInfoStatusValue.Gehistoreerd : PostInfoStatusValue.Gerealiseerd,
                    p.PostalNames.Select(x => new GeografischeNaam(x.Name, x.Language.ConvertOsloFromLanguage())),
                    p.VersionTimestamp.ToBelgianDateTimeOffset())).ToList();

            return Ok(new PostalInformationListOsloV3Response
            {
                PostInfoObjecten = items,
                Volgende = BuildNextUri(pagedPostalInformationSet.PaginationInfo, items.Count, responseOptions.Value.VolgendeUrl),
                Context = responseOptions.Value.ContextUrlList
            });
        }

        /// <summary>
        /// Vraag het totaal aantal van actieve postcodes op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="nuts3Service"></param>
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
            [FromServices] Nuts3Service nuts3Service,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalInformationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new PostalInformationListOsloQuery(context, syndicationContext, nuts3Service)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : await context
                            .PostalInformation
                            .CountAsync(cancellationToken)
                });
        }

        [HttpGet("wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [OutputCache(
            VaryByQueryKeys = ["page"],
            VaryByHeaderNames = [ExtractFilteringRequestExtension.HeaderName])]
        [ProducesResponseType(typeof(List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Changes(
            [FromServices] FeedContext context,
            [FromQuery] int? page,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalFeedFilter>();
            if (page is null)
                page = filtering.Filter?.Page ?? 1;

            var feedPosition = filtering.Filter?.FeedPosition;

            if (feedPosition.HasValue && filtering.Filter?.Page.HasValue == false)
            {
                page = context.PostalFeed
                    .Where(x => x.Position == feedPosition.Value)
                    .Select(x => x.Page)
                    .Distinct()
                    .AsEnumerable()
                    .DefaultIfEmpty(1)
                    .Min();
            }

            var feedItemsEvents = await context
                .PostalFeed
                .Where(x => x.Page == page)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return new ChangeFeedResult(jsonContent, feedItemsEvents.Count >= ChangeFeedService.DefaultMaxPageSize);
        }

        /// <summary>
        /// Vraag wijzigingen van een bepaalde gemeente op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="postcode">Postcode</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{postcode}/wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangesByNisCode(
            [FromServices] FeedContext context,
            [FromRoute] string postcode,
            CancellationToken cancellationToken = default)
        {
            var pagination = (PaginationRequest)Request.ExtractPaginationRequest();

            var feedItemsEvents = await context
                .PostalFeed
                .Where(x => x.PostalCode == postcode)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return Content(jsonContent, AcceptTypes.JsonCloudEventsBatch);
        }

        [HttpGet("posities")]
        [Produces(AcceptTypes.Json)]
        [ProducesResponseType(typeof(FeedPositieResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions(
            [FromServices] LegacyContext legacyContext,
            [FromServices] FeedContext feedContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalPositionFilter>();
            var response = new FeedPositieResponse();
            if (filtering.ShouldFilter && !filtering.Filter.HasMoreThanOneFilter)
            {
                if (filtering.Filter.Download.HasValue)
                {
                    var businessFeedPosition = await legacyContext
                        .PostalInformationSyndication
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .PostalFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = businessFeedPosition;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.Sync.HasValue)
                {
                    var changeFeed = await feedContext
                        .PostalFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Sync.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = filtering.Filter.Sync.Value;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.ChangeFeedId.HasValue)
                {
                    var feedItem = await feedContext
                        .PostalFeed
                        .AsNoTracking()
                        .Where(x => x.Id == filtering.Filter.ChangeFeedId.Value)
                        .Select(x => new { x.Id, x.Page, x.Position })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (feedItem is null)
                        return Ok(response);

                    response.Feed = feedItem.Position;
                    response.WijzigingenFeedPagina = feedItem.Page;
                    response.WijzigingenFeedId = feedItem.Id;
                }
            }

            return Ok(response);
        }

        private static Uri? BuildNextUri(PaginationInfo paginationInfo, int itemsInCollection, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemsInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }

        private static Uri? BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from.Value, limit))
                : null;
        }

        private async Task<PostinfoToegekendAanGemeente?> GetPostinfoDetailGemeente(
            SyndicationContext syndicationContext,
            string? nisCode,
            string gemeenteDetailUrl,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(nisCode))
                return null;

            var municipality = await syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipality is null)
            {
                return null;
            }

            var gemeenteNamen = new List<GeografischeNaam>
            {
                new GeografischeNaam(municipality.NameDutch, Taal.Nl),
                new GeografischeNaam(municipality.NameFrench, Taal.Fr),
                new GeografischeNaam(municipality.NameGerman, Taal.De),
                new GeografischeNaam(municipality.NameEnglish, Taal.En),
            };

            gemeenteNamen = gemeenteNamen.Where(g => !string.IsNullOrWhiteSpace(g.Spelling)).ToList();

            var gemeente = new PostinfoToegekendAanGemeente
            {
                Id = OsloNamespaces.Gemeente.ToPuri(nisCode!),
                Detail = string.Format(gemeenteDetailUrl, nisCode),
                Gemeentenaam = new Gemeentenaam
                {
                    Gemeentenamen = gemeenteNamen
                }
            };
            return gemeente;
        }
    }
}
