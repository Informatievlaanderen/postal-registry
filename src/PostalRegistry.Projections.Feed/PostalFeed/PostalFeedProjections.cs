namespace PostalRegistry.Projections.Feed.PostalFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using PostalInformation.Events;

    [ConnectedProjectionName("Feed endpoint postinfo")]
    [ConnectedProjectionDescription("Projectie die de postinfo data voor de postinfo cloudevent feed voorziet.")]
    public class PostalFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        public PostalFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                var document = new PostalDocument(message.Message.PostalCode, message.Message.Provenance.Timestamp);
                await context.PostalDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(PostalAttributeNames.PostalCode, null, document.PostalCode),
                ], PostalEventTypes.CreateV1);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                var oldNames = document.Document.Names.ToList();
                document.Document.Names.Add(new GeografischeNaam(message.Message.Name, MapLanguage(message.Message.Language)));
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(PostalAttributeNames.PostalNames, oldNames, document.Document.Names)
                ]);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                var oldNames = document.Document.Names.ToList();
                var name = document.Document.Names.Single(x =>
                    x.Taal == MapLanguage(message.Message.Language)
                    && x.Spelling == message.Message.Name);
                document.Document.Names.Remove(name);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(PostalAttributeNames.PostalNames, oldNames, document.Document.Names)
                ]);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                var oldMunicipalityId = string.IsNullOrEmpty(document.Document.NisCode)
                    ? null
                    : OsloNamespaces.Gemeente.ToPuri(document.Document.NisCode);

                document.Document.NisCode = message.Message.NisCode;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(
                        PostalAttributeNames.MunicipalityId,
                        oldMunicipalityId,
                        OsloNamespaces.Gemeente.ToPuri(message.Message.NisCode))
                ]);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                document.Document.NisCode = message.Message.NewNisCode;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(
                        PostalAttributeNames.MunicipalityId,
                        OsloNamespaces.Gemeente.ToPuri(message.Message.PreviousNisCode),
                        OsloNamespaces.Gemeente.ToPuri(message.Message.NewNisCode))
                ]);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                var oldStatus = document.Document.Status;
                document.Document.Status = PostInfoStatus.Gerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(
                        PostalAttributeNames.StatusName,
                        oldStatus,
                        PostInfoStatus.Gerealiseerd)
                ]);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                var oldStatus = document.Document.Status;
                document.Document.Status = PostInfoStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(
                        PostalAttributeNames.StatusName,
                        oldStatus,
                        PostInfoStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                var document = await context.PostalDocuments.FindAsync(message.Message.PostalCode, ct);
                if (document == null)
                    throw new InvalidOperationException($"Could not find document for postalcode {message.Message.PostalCode}");

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], PostalEventTypes.DeleteV1);
            });
        }

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            PostalDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType = PostalEventTypes.UpdateV1)
            where T : IHasProvenance, IMessage
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();
            var postalFeeDItem = new PostalFeedItem(
                position: message.Position,
                page: page,
                postalCode: document.PostalCode)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.PostalFeed.AddAsync(postalFeeDItem);

            var nisCodes = string.IsNullOrEmpty(document.Document.NisCode)
                ? (List<string>)[]
                : [document.Document.NisCode];
            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                postalFeeDItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PostalCode,
                document.LastChangedOnAsDateTimeOffset,
                nisCodes,
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            postalFeeDItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p => await context.PostalFeed.CountAsync(x => x.Page == p));
        }

        private static Taal MapLanguage(Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return Taal.NL;

                case Language.French:
                    return Taal.FR;

                case Language.German:
                    return Taal.DE;

                case Language.English:
                    return Taal.EN;
            }
        }
    }
}
