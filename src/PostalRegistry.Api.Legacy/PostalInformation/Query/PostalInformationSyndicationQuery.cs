namespace PostalRegistry.Api.Legacy.PostalInformation.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.PostalInformationSyndication;

    public class PostalInformationSyndicationQueryResult
    {
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public string PostalCode { get; }
        public long Position { get; }
        public string ChangeType { get; }

        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public PostalInformationStatus? Status { get; }
        public IEnumerable<PostalName> PostalNames { get; }

        public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn)
        {
            ContainsEvent = false;
            ContainsObject = false;

            PostalCode = postalCode;
            Position = position;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            LastChangedOn = lastChangedOn;
        }

        /*public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn)
            : this(postalCode,
                position,
                changeType,
                recordCreatedAt,
                lastChangedOn)
        {
            ContainsEvent = true;

        }*/

        public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames) :
            this(
                postalCode,
                position,
                changeType,
                recordCreatedAt,
                lastChangedOn)
        {
            ContainsObject = true;

            Status = status;
            PostalNames = postalNames;
        }

        /*public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames)
            : this(
                postalCode,
                position,
                changeType,
                recordCreatedAt,
                lastChangedOn,
                status,
                postalNames)
        {
            ContainsEvent = true;
        }*/
    }

    public class PostalInformationSyndicationQuery : Query<PostalInformationSyndicationItem,
        PostalInformationSyndicationFilter, PostalInformationSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public PostalInformationSyndicationQuery(LegacyContext context, EmbedValue embed)
        {
            _context = context;
            _embedEvent = embed?.Event ?? false;
            _embedObject = embed?.Object ?? false;
        }

        protected override ISorting Sorting => new PostalInformationSyndicationSorting();

        protected override Expression<Func<PostalInformationSyndicationItem, PostalInformationSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedEvent && _embedObject)
                    return syndicationItem => new PostalInformationSyndicationQueryResult(
                        syndicationItem.PostalCode,
                        syndicationItem.Position,
                        syndicationItem.ChangeType,
                        syndicationItem.RecordCreatedAt,
                        syndicationItem.LastChangedOn,
                        syndicationItem.Status,
                        syndicationItem.PostalNames);

                if (_embedEvent)
                    return syndicationItem => new PostalInformationSyndicationQueryResult(
                        syndicationItem.PostalCode,
                        syndicationItem.Position,
                        syndicationItem.ChangeType,
                        syndicationItem.RecordCreatedAt,
                        syndicationItem.LastChangedOn);

                if(_embedObject)
                    return syndicationItem => new PostalInformationSyndicationQueryResult(
                        syndicationItem.PostalCode,
                        syndicationItem.Position,
                        syndicationItem.ChangeType,
                        syndicationItem.RecordCreatedAt,
                        syndicationItem.LastChangedOn,
                        syndicationItem.Status,
                        syndicationItem.PostalNames);

                return syndicationItem => new PostalInformationSyndicationQueryResult(
                    syndicationItem.PostalCode,
                    syndicationItem.Position,
                    syndicationItem.ChangeType,
                    syndicationItem.RecordCreatedAt,
                    syndicationItem.LastChangedOn);
            }
        }

        protected override IQueryable<PostalInformationSyndicationItem> Filter(FilteringHeader<PostalInformationSyndicationFilter> filtering)
        {
            var postalInformationSet = _context
                .PostalInformationSyndication
                .OrderBy(x => x.Position)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return postalInformationSet;

            if (filtering.Filter.Position.HasValue)
                postalInformationSet = postalInformationSet.Where(item => item.Position >= filtering.Filter.Position);

            return postalInformationSet;
        }
    }

    internal class PostalInformationSyndicationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(PostalInformationSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(PostalInformationSyndicationItem.Position), SortOrder.Ascending);
    }

    public class PostalInformationSyndicationFilter
    {
        public long? Position { get; set; }
        public EmbedValue Embed  { get; set; }
    }
}
