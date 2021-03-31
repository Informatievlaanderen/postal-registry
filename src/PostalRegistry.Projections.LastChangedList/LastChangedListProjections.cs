namespace PostalRegistry.Projections.LastChangedList
{
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
        protected override string CacheKeyFormat => "legacy/postalinfo:{0}.{1}";
        protected override string UriFormat => "/v1/postcodes/{0}";

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml };

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

        private static void DoNothing() { }
    }
}
