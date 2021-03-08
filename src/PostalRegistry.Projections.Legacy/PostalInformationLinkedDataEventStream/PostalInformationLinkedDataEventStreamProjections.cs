namespace PostalRegistry.Projections.Legacy.PostalInformationLinkedDataEventStream
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalRegistry.PostalInformation.Events;
    using PostalRegistry.PostalInformation.Events.BPost;
    using PostalRegistry.PostalInformation.Events.Crab;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PostalInformationLinkedDataEventStreamProjections : ConnectedProjection<LegacyContext>
    {
        public PostalInformationLinkedDataEventStreamProjections()
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                var newPostalInformationLinkedDataEventStreamItem = new PostalInformationLinkedDataEventStreamItem
                {
                    Position = message.Position,
                    PostalCode = message.Message.PostalCode,
                    EventGeneratedAtTime = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName
                };

                newPostalInformationLinkedDataEventStreamItem.SetObjectHash();

                await context
                    .PostalInformationLinkedDataEventStream
                    .AddAsync(newPostalInformationLinkedDataEventStreamItem, ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationLinkedDataEventStreamItem(
                    message.Message.PostalCode,
                    message,
                    x => x.Status = PostalInformationStatus.Current,
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationLinkedDataEventStreamItem(
                    message.Message.PostalCode,
                    message,
                    x => x.Status = PostalInformationStatus.Retired,
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationLinkedDataEventStreamItem(
                    message.Message.PostalCode,
                    message,
                    x => x.AddPostalName(new PostalName(message.Message.Name, message.Message.Language)),
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewPostalInformationLinkedDataEventStreamItem(
                    message.Message.PostalCode,
                    message,
                    x =>
                    {
                        var name = x.PostalNames.First(y => y.Name == message.Message.Name);
                        x.RemovePostalName(name);
                    },
                    ct);
            });
        }
    }
}
