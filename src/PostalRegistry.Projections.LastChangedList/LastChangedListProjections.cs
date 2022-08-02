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

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => await DoNothing());
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => $"legacy/postalinfo:{{{identifier}}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/postalinfo:{{{identifier}}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/postalinfo:{{{identifier}}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => $"/v1/postcodes/{{{identifier}}}",
                AcceptType.Xml => $"/v1/postcodes/{{{identifier}}}",
                AcceptType.JsonLd => $"/v2/postcodes/{{{identifier}}}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
