namespace PostalRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;

    public class PostalInfoLastChangedListRunner : LastChangedListRunner
    {
        public const string Name = "PostalInfoLastChangedListRunner";

        public PostalInfoLastChangedListRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<PostalInfoLastChangedListRunner> logger) :
            base(
                Name,
                new Projections(),
                envelopeFactory,
                logger) { }
    }
}
