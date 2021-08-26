namespace PostalRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.PostalInformationExtract;

    public class PostalRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreatePostalFiles(ExtractContext context)
        {
            var extractItems = context
                .PostalInformationExtract
                .AsNoTracking();

            var postalInformationProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(PostalInformationExtractProjections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, postalInformationProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<PostalDbaseRecord>(
                ExtractController.ZipName,
                new PostalDbaseSchema(),
                extractItems.OrderBy(x => x.PostalCode).Select(org => org.DbaseRecord),
                extractItems.Count);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractController.ZipName,
                extractMetadata);
        }
    }
}
