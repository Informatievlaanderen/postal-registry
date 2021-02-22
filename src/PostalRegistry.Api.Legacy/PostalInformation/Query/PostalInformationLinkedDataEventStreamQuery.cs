using Microsoft.EntityFrameworkCore;
using NodaTime;
using PostalRegistry.Projections.Legacy;
using PostalRegistry.Projections.Legacy.PostalInformationSyndication;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<PostalName> PostalNames { get; }


        public PostalInformationLinkedDataEventStreamQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn)
        {
            PostalCode = postalCode;
            Position = position;
            ChangeType = changeType;
            RecordCreatedAt = recordCreatedAt;
            LastChangedOn = lastChangedOn;
        }

        public PostalInformationLinkedDataEventStreamQueryResult(
            string postalCode,
            long position,
            string changeType,
            Instant recordCreatedAt,
            Instant lastChangedOn,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames) : this(
                postalCode,
                position,
                changeType,
                recordCreatedAt,
                lastChangedOn)
        {
            Status = status;
            PostalNames = postalNames;
        }
    }

    public class PostalInformationLinkedDataEventStreamQuery
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

        public IQueryable<PostalInformationSyndicationItem> GetPage()
        {
            var offset = ((_pageNumber - 1) * _pageSize);
            var postalInformationSet = _context
                .PostalInformationSyndication
                .OrderBy(x => x.Position)
                .Skip(offset)
                .Take(_pageSize)
                .AsNoTracking();

            return postalInformationSet;
        }
    }
}
