namespace PostalRegistry.Tests.AggregateTests.WhenDeletingPostalInformation
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using PostalInformation;
    using PostalInformation.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoPostalInformation : PostalRegistryTest
    {
        private readonly Fixture _fixture;

        public GivenNoPostalInformation(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedPostalCode());
            _fixture.Customize(new WithIntegerNisCode());
        }

        [Fact]
        public void ThenAggregateNotFoundExceptionIsThrown()
        {
            var command = _fixture.Create<DeletePostalInformation>();

            Assert(
                new Scenario()
                    .Given()
                    .When(command)
                    .Throws(new AggregateNotFoundException(command.PostalCode.ToString(), typeof(PostalInformation))));
        }
    }
}
