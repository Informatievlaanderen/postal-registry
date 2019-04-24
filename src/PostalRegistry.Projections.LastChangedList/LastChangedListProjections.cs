namespace PostalRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalInformation.Events;

    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        protected override string CacheKeyFormat => "legacy/postalinfo:{0}.{1}";
        protected override string UriFormat => "/v1/postcodes/{0}";

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml };

        public LastChangedListProjections()
         : base(SupportedAcceptTypes)
        {
            When<Envelope<PostalInformationBecameCurrent>>(async (context, message, ct) =>
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
        }
    }
}
