namespace PostalRegistry.Api.Oslo.PostalInformation
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Asp.Versioning;
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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
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
    using Nuts;
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
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="nuts3Service"></param>
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
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
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
                new PostalInformationOsloResponse(
                    responseOptions.Value.Naamruimte,
                    responseOptions.Value.ContextUrlDetail,
                    postalCode,
                    gemeente,
                    postalInformation.VersionTimestamp.ToBelgianDateTimeOffset(),
                    postalInformation.IsRetired
                        ? PostInfoStatus.Gehistoreerd
                        : PostInfoStatus.Gerealiseerd,
                    nuts3Record?.Nuts3Code)
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
        /// <param name="responseOptions"></param>
        /// <param name="nuts3Service"></param>
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
            [FromServices] IOptions<ResponseOptions> responseOptions,
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
                .Select(p => new PostalInformationListItemOsloResponse(
                    p.PostalCode,
                    responseOptions.Value.Naamruimte,
                    responseOptions.Value.DetailUrl,
                    p.IsRetired ? PostInfoStatus.Gehistoreerd : PostInfoStatus.Gerealiseerd,
                    p.VersionTimestamp.ToBelgianDateTimeOffset())
                {
                    Postnamen = p.PostalNames.Select(x => x.ConvertFromPostalName()).ToList()
                }).ToList();

            return Ok(new PostalInformationListOsloResponse
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

         /// <summary>
        /// Vraag een lijst met wijzigingen van postinfo op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PostalInformationSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<PostalInformationSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var lastFeedUpdate = await context
                .PostalInformationSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var pagedPostalInformationSet =
                new PostalInformationSyndicationQuery(
                    context,
                    filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            return new ContentResult
            {
                Content = await BuildAtomFeed(lastFeedUpdate, pagedPostalInformationSet, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static async Task<string> BuildAtomFeed(
            DateTimeOffset lastFeedUpdate,
            PagedQueryable<PostalInformationSyndicationQueryResult> pagedPostalInfoItems,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");
                var atomFeedConfig = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastFeedUpdate);

                await writer.WriteDefaultMetadata(atomFeedConfig);

                var postalInfos = pagedPostalInfoItems.Items.ToList();

                var nextFrom = postalInfos.Any()
                    ? postalInfos.Max(x => x.Position) + 1
                    : (long?) null;

                var nextUri = BuildNextSyncUri(pagedPostalInfoItems.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, GrArAtomLinkTypes.Next));

                foreach (var postalInfo in postalInfos)
                    await writer.WritePostalInfo(responseOptions, formatter, syndicationConfiguration["Category"], postalInfo);

                xmlWriter.Flush();
            }

            return sw.ToString();
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

        private async Task<PostinfoDetailGemeente?> GetPostinfoDetailGemeente(
            SyndicationContext syndicationContext,
            string? nisCode,
            string gemeenteDetailUrl,
            CancellationToken ct)
        {
            var municipality = await syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipality is null)
            {
                return null;
            }

            var municipalityDefaultName = municipality.DefaultName;
            var gemeente = new PostinfoDetailGemeente
            {
                ObjectId = nisCode,
                Detail = string.Format(gemeenteDetailUrl, nisCode),
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipalityDefaultName.Value, municipalityDefaultName.Key))
            };
            return gemeente;
        }
    }
}
