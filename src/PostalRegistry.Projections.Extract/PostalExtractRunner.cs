namespace PostalRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;
    using PostalInformationExtract;

    public class PostalExtractRunner : Runner<ExtractContext>
    {
        public const string Name = "PostalExtractRunner";

        public PostalExtractRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<PostalExtractRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new PostalInformationExtractProjections(DbaseCodePage.Western_European_ANSI.ToEncoding())) { }
    }
}
