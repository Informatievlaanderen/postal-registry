namespace PostalRegistry.Projections.LastChangedList
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalInformation.Events;
    using PostalInformation.Events.BPost;
    using PostalInformation.Events.Crab;

    [ConnectedProjectionName("Cache markering postinfo")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel postinfo de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.JsonLd };

        public LastChangedListProjections()
         : base(SupportedAcceptTypes)
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);

                foreach (var record in attachedRecords)
                {
                    record.CacheKey = string.Format(record.CacheKey, message.Message.PostalCode);
                    record.Uri = string.Format(record.Uri, message.Message.PostalCode);
                }
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });

            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => await DoNothing());

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
            });
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                 AcceptType.JsonLd => string.Format("oslo/postalinfo:{{0}}.{1}", identifier, shortenedAcceptType),
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => string.Format("/v2/postcodes/{{0}}", identifier),
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
