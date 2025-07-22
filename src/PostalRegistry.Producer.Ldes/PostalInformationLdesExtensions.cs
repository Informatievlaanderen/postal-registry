namespace PostalRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class PostalInformationLdesExtensions
    {
        public static async Task FindAndUpdatePostalInformationDetail(
            this ProducerContext context,
            string postalCode,
            Action<PostalInformationDetail> updateFunc,
            CancellationToken ct)
        {
            var postalInformation = await context
                .PostalInformations
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalInformation is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(postalCode);
            }

            updateFunc(postalInformation);
        }

        public static async Task DeletePostalInformationDetail(
            this ProducerContext context,
            string postalCode,
            CancellationToken ct)
        {
            var postalInformation = await context
                .PostalInformations
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalInformation is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(postalCode);
            }

            context.PostalInformations.Remove(postalInformation);
        }
    }
}
