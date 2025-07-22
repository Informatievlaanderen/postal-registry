namespace PostalRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Domain = PostalInformation.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationPostalNameWasAdded>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationPostalNameWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.MunicipalityWasAttached>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.MunicipalityWasRelinked>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Domain.PostalInformationWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PostalCode, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(
            string postalCode,
            T message,
            long storePosition,
            CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(postalCode),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
