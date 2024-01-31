namespace PostalRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using PostalInformation.Events;

    public class PostalInformationPostalNameWasRemovedBuilder
    {
        private PostalName? _name;
        private readonly Fixture _fixture;

        public PostalInformationPostalNameWasRemovedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public PostalInformationPostalNameWasRemovedBuilder WithName(string name, Language language)
        {
            _name = new PostalName(name, language);

            return this;
        }

        public PostalInformationPostalNameWasRemoved Build()
        {
            var @event = new PostalInformationPostalNameWasRemoved(
                _fixture.Create<PostalCode>(),
                _name ?? _fixture.Create<PostalName>());
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            return @event;
        }
    }
}
