namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class PostalInformationExtractExtensions
    {
        public static async Task<List<PostalInformationExtractItem>> FindAndUpdatePostalInformationExtract(
            this ExtractContext context,
            string postalCode,
            Action<List<PostalInformationExtractItem>> updateFunc,
            CancellationToken ct)
        {
            var postalInformationSet = (await context.AllVersions(postalCode, ct)).ToList();

            if (postalInformationSet == null || postalInformationSet.Count == 0)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(postalInformationSet);

            return postalInformationSet;
        }

        public static async Task<IEnumerable<PostalInformationExtractItem>> AllVersions(
            this ExtractContext context,
            string postalCode,
            CancellationToken cancellationToken)
        {
            var sqlEntities = await context
                .PostalInformationExtract
                .Where(x => x.PostalCode == postalCode)
                .ToListAsync(cancellationToken);

            var localEntities = context
                .PostalInformationExtract
                .Local
                .Where(x => x.PostalCode == postalCode)
                .ToList();

            return sqlEntities
                .Union(localEntities)
                .Distinct();
        }

        private static ProjectionItemNotFoundException<PostalInformationExtractProjections> DatabaseItemNotFound(string postalId)
            => new ProjectionItemNotFoundException<PostalInformationExtractProjections>(postalId);
    }
}
