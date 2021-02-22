namespace PostalRegistry.Projections.Legacy.PostalInformationSyndication
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalRegistry.PostalInformation.Events;
    using PostalRegistry.PostalInformation.Events.BPost;
    using PostalRegistry.PostalInformation.Events.Crab;

    public class PostalInformationSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public PostalInformationSyndicationProjections()
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                var newPostalInformationSyndicationItem = new PostalInformationSyndicationItem
                {
                    Position = message.Position,
                    PostalCode = message.Message.PostalCode,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                //newPostalInformationSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newPostalInformationSyndicationItem.SetEventData<PostalInformationSyndicationItem>();

                await context
                    .PostalInformationSyndication
                    .AddAsync(newPostalInformationSyndicationItem, ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.Status = PostalInformationStatus.Current,
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.Status = PostalInformationStatus.Retired,
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.AddPostalName(new PostalName(message.Message.Name, message.Message.Language)),
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x =>
                    {
                        var name = x.PostalNames.First(y => y.Name == message.Message.Name);
                        x.RemovePostalName(name);
                    },
                    ct);
            });

            /*When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.MunicipalityNisCode = message.Message.NisCode,
                    ct);
            });*/
            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) => DoNothing());

            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => DoNothing());
        }

        private static void DoNothing() { }
    }
}
