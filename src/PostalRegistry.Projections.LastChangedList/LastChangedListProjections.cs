namespace PostalRegistry.Projections.LastChangedList
{
    using System;
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

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

        public LastChangedListProjections()
         : base(SupportedAcceptTypes)
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PostalCode, message.Position, context, ct);
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

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => DoNothing());
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => string.Format("legacy/postalinfo:{{0}}.{1}", identifier, shortenedAcceptType),
                AcceptType.Xml => string.Format("legacy/postalinfo:{{0}}.{1}", identifier, shortenedAcceptType),
                AcceptType.JsonLd => string.Format("oslo/postalinfo:{{0}}.{1}", identifier, shortenedAcceptType),
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => string.Format("/v1/postcodes/{{0}}", identifier),
                AcceptType.Xml => string.Format("/v1/postcodes/{{0}}", identifier),
                AcceptType.JsonLd => string.Format("/v2/postcodes/{{0}}", identifier),
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static void DoNothing() { }
    }
}
