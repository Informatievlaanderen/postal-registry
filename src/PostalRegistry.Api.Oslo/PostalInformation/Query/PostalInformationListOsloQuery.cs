namespace PostalRegistry.Api.Oslo.PostalInformation.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.PostalInformation;
    using Projections.Syndication;

    public class PostalInformationListOsloQuery : Query<PostalInformation, PostalInformationFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new PostalInformationSorting();

        public PostalInformationListOsloQuery(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<PostalInformation> Filter(FilteringHeader<PostalInformationFilter> filtering)
        {
            var postalInformationSet = _context
                .PostalInformation
                .AsNoTracking()
                .OrderBy(x => x.PostalCode)
                .Where(p => !p.IsRetired);

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
    }
}
