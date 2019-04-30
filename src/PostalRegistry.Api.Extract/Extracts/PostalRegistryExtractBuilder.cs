namespace PostalRegistry.Api.Extract.Extracts
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;

    public class PostalRegistryExtractBuilder
    {
        public static ExtractFile CreatePostalFile(ExtractContext context)
        {
            var extractItems = context
                .PostalInformationExtract
                .AsNoTracking();

            return ExtractBuilder.CreateDbfFile<PostalDbaseRecord>(
                ExtractController.ZipName,
                new PostalDbaseSchema(),
                extractItems.OrderBy(x => x.PostalCode).Select(org => org.DbaseRecord),
                extractItems.Count);
        }
    }
}
