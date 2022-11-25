namespace PostalRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Domain = PostalInformation.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string topicKey = "Topic";

        public ProducerProjections(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var topic = $"{configuration[topicKey]}" ?? throw new ArgumentException($"Configuration has no value for {topicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                false,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), ct);
            });
        }

        private async Task Produce<T>(string postalCode, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, postalCode, message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
