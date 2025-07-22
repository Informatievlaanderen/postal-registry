namespace PostalRegistry.Api.Oslo.PostalInformation.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.EntityFrameworkCore;
    using Nuts;
    using Projections.Legacy;
    using Projections.Legacy.PostalInformation;
    using Projections.Syndication;

    public class PostalInformationListOsloQuery : Query<PostalInformation, PostalInformationFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly Nuts3Service _nuts3Service;

        protected override ISorting Sorting => new PostalInformationSorting();

        public PostalInformationListOsloQuery(
            LegacyContext context,
            SyndicationContext syndicationContext,
            Nuts3Service nuts3Service)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _nuts3Service = nuts3Service;
        }

        protected override IQueryable<PostalInformation> Filter(FilteringHeader<PostalInformationFilter> filtering)
        {
            var postalInformationSet = _context
                .PostalInformation
                .AsNoTracking()
                .OrderBy(x => x.PostalCode)
                .Where(p => !p.IsRetired && !p.IsRemoved);

            if (!filtering.ShouldFilter)
                return postalInformationSet;

            var filterMunicipalityName = filtering.Filter.MunicipalityName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filterMunicipalityName))
            {
                var municipalityNisCodes = _syndicationContext
                    .MunicipalityLatestItems
                    .AsNoTracking()
                    .Where(x => x.NameDutchSearch == filterMunicipalityName ||
                                x.NameFrenchSearch == filterMunicipalityName ||
                                x.NameEnglishSearch == filterMunicipalityName ||
                                x.NameGermanSearch == filterMunicipalityName)
                    .Select(x => x.NisCode)
                    .ToList();

                postalInformationSet = postalInformationSet.Where(x => municipalityNisCodes.Contains(x.NisCode));
            }

            var filterPostalName = filtering.Filter.PostalName.RemoveDiacritics();
            if (!string.IsNullOrEmpty(filterPostalName))
            {
                postalInformationSet = postalInformationSet
                    .Where(x => x.PostalNames.Any(y => y.SearchName == filterPostalName));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Nuts3Code))
            {
                var nuts3PostalCodes = _nuts3Service.GetPostalCodesByNuts3(filtering.Filter.Nuts3Code)
                    .Select(x => x.PostalCode)
                    .ToList();

                if(!nuts3PostalCodes.Any())
                    return Enumerable.Empty<PostalInformation>().AsQueryable();

                postalInformationSet = postalInformationSet.Where(x => nuts3PostalCodes.Any(y => y == x.PostalCode));
            }

            if (filtering.Filter.HasMunicipality.HasValue)
            {
                postalInformationSet = filtering.Filter.HasMunicipality.Value
                    ? postalInformationSet.Where(x => x.NisCode != null)
                    : postalInformationSet.Where(x => x.NisCode == null);
            }

            return postalInformationSet;
        }
    }

    internal class PostalInformationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(PostalInformation.PostalCode)
        };

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(PostalInformation.PostalCode), SortOrder.Ascending);
    }

    public class PostalInformationFilter
    {
        public string MunicipalityName { get; set; }

        public string PostalName { get; set; }

        public string Nuts3Code { get; set; }

        public bool? HasMunicipality { get; set; }
    }
}
