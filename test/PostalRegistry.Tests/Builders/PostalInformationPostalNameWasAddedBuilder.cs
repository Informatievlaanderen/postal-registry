namespace PostalRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using PostalInformation.Events;

    public class PostalInformationPostalNameWasAddedBuilder
    {
        private PostalName? _name;
        private readonly Fixture _fixture;

        public PostalInformationPostalNameWasAddedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public PostalInformationPostalNameWasAddedBuilder WithName(string name, Language language)
        {
            _name = new PostalName(name, language);

            return this;
        }

        public PostalInformationPostalNameWasAdded Build()
        {
            var @event = new PostalInformationPostalNameWasAdded(
                _fixture.Create<PostalCode>(),
                _name ?? _fixture.Create<PostalName>());
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            return @event;
        }
    }
}
