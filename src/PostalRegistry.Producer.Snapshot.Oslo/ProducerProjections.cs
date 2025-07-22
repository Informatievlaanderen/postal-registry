namespace PostalRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using PostalInformation.Events;

    [ConnectedProjectionName("Kafka producer snapshot oslo")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;
        private readonly string _osloNamespace;

        public ProducerProjections(IProducer producer, ISnapshotManager snapshotManager, string osloNamespace)
        {
            _producer = producer;
            _osloNamespace = osloNamespace;

            When<Envelope<PostalInformationWasRegistered>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<PostalInformationWasRemoved>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.PostalCode}", message.Message.PostalCode, "{}", message.Position, ct);

            });

            When<Envelope<MunicipalityWasAttached>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PostalCode,
                        message.Message.Provenance.Timestamp,
                        null,
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            null,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });
        }

        private async Task FindAndProduce(
            Func<Task<OsloResult?>> findMatchingSnapshot,
            long storePosition,
            CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
        }

        private async Task Produce(
            string puri,
            string objectId,
            string jsonContent,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
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
