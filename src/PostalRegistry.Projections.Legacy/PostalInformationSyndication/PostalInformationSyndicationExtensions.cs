namespace PostalRegistry.Projections.Legacy.PostalInformationSyndication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class PostalInformationSyndicationExtensions
    {
        public static async Task CreateNewPostalInformationSyndicationItem<T>(
            this LegacyContext context,
            string postalCode,
            Envelope<T> message,
            Action<PostalInformationSyndicationItem> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var postalInformationSyndicationItem = await context.LatestPosition(postalCode, ct);

            if (postalInformationSyndicationItem == null)
                throw DatabaseItemNotFound(postalCode);

            var provenance = message.Message.Provenance;

            var newPostalInformationSyndicationItem = postalInformationSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newPostalInformationSyndicationItem.ApplyProvenance(provenance);
            newPostalInformationSyndicationItem.SetEventData(message.Message, message.EventName);

            await context
                .PostalInformationSyndication
                .AddAsync(newPostalInformationSyndicationItem, ct);
        }

        public static async Task<PostalInformationSyndicationItem> LatestPosition(
            this LegacyContext context,
            string postalCode,
            CancellationToken ct)
            => context
                   .PostalInformationSyndication
                   .Local
                   .Where(x => x.PostalCode == postalCode)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .PostalInformationSyndication
                   .Where(x => x.PostalCode == postalCode)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static void ApplyProvenance(
            this PostalInformationSyndicationItem item,
            ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        public static void SetEventData<T>(this PostalInformationSyndicationItem syndicationItem, T message, string eventName)
            => syndicationItem.EventDataAsXml = message.ToXml(eventName).ToString(SaveOptions.DisableFormatting);

        private static ProjectionItemNotFoundException<PostalInformationSyndicationProjections> DatabaseItemNotFound(string postalCode)
            => new ProjectionItemNotFoundException<PostalInformationSyndicationProjections>(postalCode);
    }
}
