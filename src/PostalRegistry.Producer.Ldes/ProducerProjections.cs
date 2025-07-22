namespace PostalRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Newtonsoft.Json;
    using PostalInformation.Events;

    [ConnectedProjectionName("Kafka producer Ldes")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;
        private readonly string _osloNamespace;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ProducerProjections(
            IProducer producer,
            string osloNamespace,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _producer = producer;
            _osloNamespace = osloNamespace.Trim('/');
            _jsonSerializerSettings = jsonSerializerSettings;

            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await context.PostalInformations.AddAsync(new PostalInformationDetail
                {
                    PostalCode = message.Message.PostalCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    switch (message.Message.Language)
                    {
                        case Language.Dutch:
                            postalInformation.NamesDutch = postalInformation.NamesDutch
                                .Concat([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.French:
                            postalInformation.NamesFrench = postalInformation.NamesFrench
                                .Concat([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.English:
                            postalInformation.NamesEnglish = postalInformation.NamesEnglish
                                .Concat([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.German:
                            postalInformation.NamesGerman = postalInformation.NamesGerman
                                .Concat([message.Message.Name])
                                .ToArray();
                            break;
                    }

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    switch (message.Message.Language)
                    {
                        case Language.Dutch:
                            postalInformation.NamesDutch = postalInformation.NamesDutch
                                .Except([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.French:
                            postalInformation.NamesFrench = postalInformation.NamesFrench
                                .Except([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.English:
                            postalInformation.NamesEnglish = postalInformation.NamesEnglish
                                .Except([message.Message.Name])
                                .ToArray();
                            break;
                        case Language.German:
                            postalInformation.NamesGerman = postalInformation.NamesGerman
                                .Except([message.Message.Name])
                                .ToArray();
                            break;
                    }

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    postalInformation.IsRetired = false;

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    postalInformation.IsRetired = true;

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                await context.DeletePostalInformationDetail(message.Message.PostalCode, ct);
                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    postalInformation.NisCode = message.Message.NisCode;

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationDetail(message.Message.PostalCode, postalInformation =>
                {
                    postalInformation.NisCode = message.Message.NewNisCode;

                    postalInformation.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);

                await Produce(context, message.Message.PostalCode, message.Position, ct);
            });
        }

        private async Task Produce(
            ProducerContext context,
            string postalCode,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var postalInformation = await context.PostalInformations.FindAsync(postalCode, cancellationToken: cancellationToken)
                                    ?? throw new ProjectionItemNotFoundException<ProducerProjections>(postalCode);

            if (postalInformation.NisCode is null || !RegionFilter.IsFlemishRegion(postalInformation.NisCode))
            {
                return;
            }

            var postalInformationLdes = new PostalInformationLdes(postalInformation, _osloNamespace);

            await Produce(
                postalInformation.PostalCode,
                JsonConvert.SerializeObject(postalInformationLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task Produce(
            string objectId,
            string jsonContent,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var puri = $"{_osloNamespace}/{objectId}";

            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
