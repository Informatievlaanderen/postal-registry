namespace PostalRegistry.Projections.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class PostalLatestItemExtensions
    {
        public static async Task<PostalLatestItem> FindAndUpdatePostal(this IntegrationContext context,
            string postalCode,
            Action<PostalLatestItem> updateFunc,
            CancellationToken ct)
        {
                var postalItem = await context
                    .PostalLatestItems
                    .Include(x=> x.PostalNames)
                    .SingleOrDefaultAsync(x => x.PostalCode == postalCode, cancellationToken: ct);

                if (postalItem == null)
                    throw DatabaseItemNotFound(postalCode);

                updateFunc(postalItem);

                return postalItem;
            }

        public static async Task DeletePostal(this IntegrationContext context,
            string postalCode,
            CancellationToken ct)
        {
            var postalItem = await context.PostalLatestItems
                .Include(x => x.PostalNames)
                .SingleOrDefaultAsync(x => x.PostalCode == postalCode, cancellationToken: ct);

            if (postalItem == null)
                throw DatabaseItemNotFound(postalCode);

            context.PostalLatestItems.Remove(postalItem);
        }

            private static ProjectionItemNotFoundException<PostalLatestItemProjections> DatabaseItemNotFound(string postalCode)
                => new ProjectionItemNotFoundException<PostalLatestItemProjections>(postalCode);
    }
}
