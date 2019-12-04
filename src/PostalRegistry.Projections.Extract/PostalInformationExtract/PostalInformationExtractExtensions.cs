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
            var postalInfo = await context
                .PostalInformationExtract
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalInfo == null)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(postalInfo);

            return postalInfo;
        }

        private static ProjectionItemNotFoundException<PostalInformationExtractProjections> DatabaseItemNotFound(string postalId)
            => new ProjectionItemNotFoundException<PostalInformationExtractProjections>(postalId);
    }
}
