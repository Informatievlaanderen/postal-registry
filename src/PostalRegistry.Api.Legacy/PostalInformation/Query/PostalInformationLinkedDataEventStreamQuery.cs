namespace PostalRegistry.Api.Legacy.PostalInformation.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using PostalRegistry.Projections.Legacy;
    using PostalRegistry.Projections.Legacy.PostalInformationLinkedDataEventStream;
    using PostalRegistry.Projections.Legacy.PostalInformationSyndication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class PostalInformationLinkedDataEventStreamQueryResult
    {
        public string PostalCode { get; }
        public long Position { get; }
        public string ChangeType { get; }
        public Instant RecordCreatedAt { get; }
        public PostalInformationStatus? Status { get; }
        public IEnumerable<PostalName>? PostalNames { get; }

        public PostalInformationLinkedDataEventStreamQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            PostalInformationStatus? status,
            IEnumerable<PostalName>? postalNames)
        {
            PostalCode = postalCode;
            Position = position;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            Status = status;
            PostalNames = postalNames;
        }
    }

    public class PostalInformationLinkedDataEventStreamQuery : Query<PostalInformationLinkedDataEventStreamItem, PostalInformationLinkedDataEventStreamFilter, PostalInformationLinkedDataEventStreamQueryResult>
    {
        private readonly LegacyContext _context;

        public PostalInformationLinkedDataEventStreamQuery(LegacyContext context) => _context = context;

        protected override ISorting Sorting => new PostalInformationLDESSorting();

        protected override Expression<Func<PostalInformationLinkedDataEventStreamItem, PostalInformationLinkedDataEventStreamQueryResult>> Transformation
        {
            get
            {
                return syndicationItem => new PostalInformationLinkedDataEventStreamQueryResult(
                        syndicationItem.PostalCode,
                        syndicationItem.Position,
                        syndicationItem.ChangeType,
                        syndicationItem.RecordCreatedAt,
                        syndicationItem.Status,
                        syndicationItem.PostalNames);
            }
        }

        protected override IQueryable<PostalInformationLinkedDataEventStreamItem> Filter(FilteringHeader<PostalInformationLinkedDataEventStreamFilter> filtering)
        {
            return _context
                .PostalInformationLinkedDataEventStream
                .OrderBy(x => x.Position)
                .AsNoTracking();
        }
    }

    internal class PostalInformationLDESSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(PostalInformationLinkedDataEventStreamItem.Position)
        };
        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(PostalInformationLinkedDataEventStreamItem.Position), SortOrder.Ascending);
    }

    public class PostalInformationLinkedDataEventStreamFilter
    {
        public int PageNumber { get; set; }
    }
}
