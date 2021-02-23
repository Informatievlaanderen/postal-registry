using Be.Vlaanderen.Basisregisters.Api.Search;
using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using PostalRegistry.Projections.Legacy;
using PostalRegistry.Projections.Legacy.PostalInformationSyndication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PostalRegistry.Api.Legacy.PostalInformation.Query
{
    public class PostalInformationLinkedDataEventStreamQueryResult
    {
        public string PostalCode { get; }
        public long Position { get; }
        public string ChangeType { get; }

        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public PostalInformationStatus? Status { get; }
        public IEnumerable<PostalName>? PostalNames { get; }

        public string EventDataAsJsonLd { get; }


        public PostalInformationLinkedDataEventStreamQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            PostalInformationStatus? status,
            IEnumerable<PostalName>? postalNames,
            string eventDataAsJsonLd)
        {
            PostalCode = postalCode;
            Position = position;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            LastChangedOn = lastChangedOn;
            Status = status;
            PostalNames = postalNames;
            EventDataAsJsonLd = eventDataAsJsonLd;
        }
    }

    public class PostalInformationLinkedDataEventStreamQuery : Query<PostalInformationSyndicationItem, PostalInformationLDESFilter, PostalInformationLinkedDataEventStreamQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly int _pageNumber;
        private readonly int _pageSize;

        public PostalInformationLinkedDataEventStreamQuery(LegacyContext context, int pageNumber, int pageSize)
        {
            _context = context;
            _pageNumber = pageNumber;
            _pageSize = pageSize;
        }

        protected override ISorting Sorting => new PostalInformationLDESSorting();

        protected override Expression<Func<PostalInformationSyndicationItem, PostalInformationLinkedDataEventStreamQueryResult>> Transformation
        {
            get
            {
                return syndicationItem => new PostalInformationLinkedDataEventStreamQueryResult(
                        syndicationItem.PostalCode,
                        syndicationItem.Position,
                        syndicationItem.ChangeType,
                        syndicationItem.RecordCreatedAt,
                        syndicationItem.LastChangedOn,
                        syndicationItem.Status,
                        syndicationItem.PostalNames,
                        syndicationItem.EventDataAsJsonLd);
            }
        }

        protected override IQueryable<PostalInformationSyndicationItem> Filter(FilteringHeader<PostalInformationLDESFilter> filtering)
        {
            int offset = ((_pageNumber - 1) * _pageSize);
            var postalInformationSet = _context
                .PostalInformationSyndication
                .OrderBy(x => x.Position)
                .Skip(offset)
                .Take(_pageSize)
                .AsNoTracking();


            return postalInformationSet;
        }
    }

    internal class PostalInformationLDESSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(PostalInformationSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(PostalInformationSyndicationItem.Position), SortOrder.Ascending);
    }

    public class PostalInformationLDESFilter
    {
        public int PageNumber { get; set; }
    }
}
