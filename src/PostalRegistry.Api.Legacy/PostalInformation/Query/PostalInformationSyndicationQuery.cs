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
        public bool ContainsDetails { get; }

        public string PostalCode { get; }
        public long Position { get; }
        public string ChangeType { get; }

        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public PostalInformationStatus? Status { get; }
        public IEnumerable<PostalName> PostalNames { get; }
        public string MunicipalityOsloId { get; }
        public Organisation? Organisation { get; }
        public Plan? Plan { get; }

        public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            string municipalityOsloId,
            Organisation? organisation,
            Plan? plan)
        {
            ContainsDetails = false;

            PostalCode = postalCode;
            Position = position;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            LastChangedOn = lastChangedOn;
            MunicipalityOsloId = municipalityOsloId;
            Organisation = organisation;
            Plan = plan;
        }

        public PostalInformationSyndicationQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames,
            string municipalityOsloId,
            Organisation? organisation,
            Plan? plan) :
            this(
                postalCode,
                position,
                changeType,
                recordCreatedAt,
                lastChangedOn,
                municipalityOsloId,
                organisation,
                plan)
        {
            ContainsDetails = true;

            Status = status;
            PostalNames = postalNames;
        }
    }

    public class PostalInformationSyndicationQuery : Query<PostalInformationSyndicationItem, PostalInformationSyndicationFilter, PostalInformationSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embed;

        public PostalInformationSyndicationQuery(LegacyContext context, bool embed)
        {
            _context = context;
            _embed = embed;
        }

        protected override ISorting Sorting => new PostalInformationSyndicationSoring();

        protected override Expression<Func<PostalInformationSyndicationItem, PostalInformationSyndicationQueryResult>> Transformation => _embed
            ? (Expression<Func<PostalInformationSyndicationItem, PostalInformationSyndicationQueryResult>>) (syndicationItem =>
                new PostalInformationSyndicationQueryResult(
                    syndicationItem.PostalCode,
                    syndicationItem.Position,
                    syndicationItem.ChangeType,
                    syndicationItem.RecordCreatedAt,
                    syndicationItem.LastChangedOn,
                    syndicationItem.Status,
                    syndicationItem.PostalNames,
                    syndicationItem.MunicipalityOsloId,
                    syndicationItem.Organisation,
                    syndicationItem.Plan))
            : syndicationItem =>
                new PostalInformationSyndicationQueryResult(
                    syndicationItem.PostalCode,
                    syndicationItem.Position,
                    syndicationItem.ChangeType,
                    syndicationItem.RecordCreatedAt,
                    syndicationItem.LastChangedOn,
                    syndicationItem.MunicipalityOsloId,
                    syndicationItem.Organisation,
                    syndicationItem.Plan);

        protected override IQueryable<PostalInformationSyndicationItem> Filter(FilteringHeader<PostalInformationSyndicationFilter> filtering)
        {
            var postalInformationSet = _context
                .PostalInformationSyndication
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return postalInformationSet;

            if (filtering.Filter.Position.HasValue)
                postalInformationSet = postalInformationSet.Where(item => item.Position >= filtering.Filter.Position);

            return postalInformationSet;
        }
    }

    internal class PostalInformationSyndicationSoring : ISorting
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
    }
}
