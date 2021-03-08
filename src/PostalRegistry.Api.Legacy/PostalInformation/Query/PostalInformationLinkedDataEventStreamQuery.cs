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
        public string ChangeType { get; }
        public Instant EventGeneratedAtTime { get; }
        public PostalInformationStatus? Status { get; }
        public IEnumerable<PostalName>? PostalNames { get; }
        public string ObjectIdentifier { get; }

        public PostalInformationLinkedDataEventStreamQueryResult(
            string postalCode,
            string objectIdentifier,
            string changeType,
            Instant eventGeneratedAtTime,
            PostalInformationStatus? status,
            IEnumerable<PostalName>? postalNames)
        {
            PostalCode = postalCode;
            ObjectIdentifier = objectIdentifier;
            ChangeType = changeType;
            EventGeneratedAtTime = eventGeneratedAtTime;
            Status = status;
            PostalNames = postalNames;
        }
    }

    public class PostalInformationLinkedDataEventStreamQuery : Query<PostalInformationLinkedDataEventStreamItem, PostalInformationLinkedDataEventStreamFilter, PostalInformationLinkedDataEventStreamQueryResult>
    {
        private readonly LegacyContext _context;

        public PostalInformationLinkedDataEventStreamQuery(LegacyContext context)
            => _context = context;

        protected override ISorting Sorting => new PostalInformationLinkedDataEventStreamSorting();

        protected override Expression<Func<PostalInformationLinkedDataEventStreamItem, PostalInformationLinkedDataEventStreamQueryResult>> Transformation
        {
            get
            {
                return linkedDataEventStreamItem => new PostalInformationLinkedDataEventStreamQueryResult(
                        linkedDataEventStreamItem.PostalCode,
                        linkedDataEventStreamItem.ObjectHash,
                        linkedDataEventStreamItem.ChangeType,
                        linkedDataEventStreamItem.EventGeneratedAtTime,
                        linkedDataEventStreamItem.Status,
                        linkedDataEventStreamItem.PostalNames);
            }
        }

        protected override IQueryable<PostalInformationLinkedDataEventStreamItem> Filter(FilteringHeader<PostalInformationLinkedDataEventStreamFilter> filtering)
            => _context
                .PostalInformationLinkedDataEventStream
                .OrderBy(x => x.Position)
                .AsNoTracking();
    }

    internal class PostalInformationLinkedDataEventStreamSorting : ISorting
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
