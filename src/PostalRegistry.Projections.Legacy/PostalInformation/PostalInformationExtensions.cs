namespace PostalRegistry.Projections.Legacy.PostalInformation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class PostalInformationExtensions
    {
        public static async Task<PostalInformation> FindAndUpdatePostalInformation(
            this LegacyContext context,
            string postalCode,
            Action<PostalInformation> updateFunc,
            CancellationToken ct)
        {
            var postalInformation = await context
                .PostalInformation
                .Include(p => p.PostalNames)
                .SingleOrDefaultAsync(x => x.PostalCode == postalCode, cancellationToken: ct);

            if (postalInformation == null)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(postalInformation);

            return postalInformation;
        }

        private static ProjectionItemNotFoundException<PostalInformationProjections> DatabaseItemNotFound(string postalCode)
            => new ProjectionItemNotFoundException<PostalInformationProjections>(postalCode);
    }
}
