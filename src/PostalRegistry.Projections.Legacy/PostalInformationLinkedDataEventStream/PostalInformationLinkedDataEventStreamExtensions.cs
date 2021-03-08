namespace PostalRegistry.Projections.Legacy.PostalInformationLinkedDataEventStream
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public static class PostalInformationLinkedDataEventStreamExtensions
    {
        public static async Task CreateNewPostalInformationLinkedDataEventStreamItem<T>(
            this LegacyContext context,
            string postalCode,
            Envelope<T> message,
            Action<PostalInformationLinkedDataEventStreamItem> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance
        {
            var postalInformationLinkedDataEventStreamItem = await context.LatestPosition(postalCode, ct);

            if (postalInformationLinkedDataEventStreamItem == null)
                throw DatabaseItemNotFound(postalCode);

            var provenance = message.Message.Provenance;

            var newItem = postalInformationLinkedDataEventStreamItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newItem.SetObjectHash();

            await context
                .PostalInformationLinkedDataEventStream
                .AddAsync(newItem, ct);
        }

        public static async Task<PostalInformationLinkedDataEventStreamItem> LatestPosition(
            this LegacyContext context,
            string postalCode,
            CancellationToken ct)
            => context
                    .PostalInformationLinkedDataEventStream
                    .Local
                    .Where(x => x.PostalCode == postalCode)
                    .OrderByDescending(x => x.Position)
                    .FirstOrDefault()
                ?? await context
                    .PostalInformationLinkedDataEventStream
                    .Where(x => x.PostalCode == postalCode)
                    .OrderByDescending(x => x.Position)
                    .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<PostalInformationLinkedDataEventStreamProjections> DatabaseItemNotFound(string postalCode)
           => new ProjectionItemNotFoundException<PostalInformationLinkedDataEventStreamProjections>(postalCode);

        public static void SetObjectHash(this PostalInformationLinkedDataEventStreamItem linkedDataEventStreamItem)
        {
            var objectString = JsonConvert.SerializeObject(linkedDataEventStreamItem);

            using var md5Hash = MD5.Create();
            var hashBytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(objectString));
            linkedDataEventStreamItem.ObjectHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }
}
