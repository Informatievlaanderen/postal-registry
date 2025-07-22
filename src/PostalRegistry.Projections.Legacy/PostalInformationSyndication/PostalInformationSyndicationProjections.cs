namespace PostalRegistry.Projections.Legacy.PostalInformationSyndication
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalRegistry.PostalInformation.Events;
    using PostalRegistry.PostalInformation.Events.BPost;
    using PostalRegistry.PostalInformation.Events.Crab;

    [ConnectedProjectionName("Feed endpoint postinfo")]
    [ConnectedProjectionDescription("Projectie die de postinfo data voor de postinfo feed voorziet.")]
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

                newPostalInformationSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newPostalInformationSyndicationItem.SetEventData(message.Message, message.EventName);

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

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.MunicipalityNisCode = message.Message.NisCode,
                    ct);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.MunicipalityNisCode = message.Message.NewNisCode,
                    ct);
            });

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationSyndicationItem(
                    message.Message.PostalCode,
                    message,
                    x => x.IsRemoved = true,
                    ct);
            });

            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => await DoNothing());
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
