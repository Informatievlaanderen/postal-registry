namespace PostalRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;
    using PostalInformation;
    using PostalInformationSyndication;

    public class PostalLegacyRunner : Runner<LegacyContext>
    {
        public const string Name = "PostalLegacyRunner";

        public PostalLegacyRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<PostalLegacyRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new PostalInformationProjections(),
                new PostalInformationSyndicationProjections()) { }
    }
}
