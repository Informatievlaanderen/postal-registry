namespace PostalRegistry.Projections.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class PostalLatestItemExtensions
    {
            public static async Task<PostalLatestItem> FindAndUpdatePostal(this IntegrationContext context,
                string postalCode,
                Action<PostalLatestItem> updateFunc,
                CancellationToken ct)
            {
                var postalItem = await context
                    .PostalLatestItems
                    .FindAsync(postalCode, cancellationToken: ct);

                if (postalItem == null)
                    throw DatabaseItemNotFound(postalCode);

                updateFunc(postalItem);

                return postalItem;
            }

            private static ProjectionItemNotFoundException<PostalLatestItemProjections> DatabaseItemNotFound(string postalCode)
                => new ProjectionItemNotFoundException<PostalLatestItemProjections>(postalCode);
    }
}
