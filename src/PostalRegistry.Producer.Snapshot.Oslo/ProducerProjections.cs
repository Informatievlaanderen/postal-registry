namespace PostalRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Configuration;
    using PostalInformation.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private const string TopicKey = "Topic";

        public ProducerProjections(IConfiguration configuration, ISnapshotManager snapshotManager)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];            

            var topic = $"{configuration[TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {TopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                false,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            When<Envelope<PostalInformationWasRegistered>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PostalCode,
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, ct);
            }
        }

        private async Task Produce(string objectId, string jsonContent, CancellationToken cancellationToken = default)
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, objectId, jsonContent, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
