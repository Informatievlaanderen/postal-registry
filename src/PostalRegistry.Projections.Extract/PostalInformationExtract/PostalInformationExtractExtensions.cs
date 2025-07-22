namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class PostalInformationExtractExtensions
    {
        public static async Task<PostalInformationExtractItem> FindAndUpdatePostalInformationExtract(
            this ExtractContext context,
            string postalCode,
            Action<PostalInformationExtractItem> updateFunc,
            CancellationToken ct)
        {
            var postalInformation = await context
                .PostalInformationExtract
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalInformation == null)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(postalInformation);

            return postalInformation;
        }

        public static async Task DeletePostalInformationExtract(
            this ExtractContext context,
            string postalCode,
            CancellationToken ct)
        {
            var postalInformation = await context
                .PostalInformationExtract
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalInformation == null)
                throw DatabaseItemNotFound(postalCode);

            context.PostalInformationExtract.Remove(postalInformation);
        }

        private static ProjectionItemNotFoundException<PostalInformationExtractProjections> DatabaseItemNotFound(string postalId)
            => new ProjectionItemNotFoundException<PostalInformationExtractProjections>(postalId);
    }
}
