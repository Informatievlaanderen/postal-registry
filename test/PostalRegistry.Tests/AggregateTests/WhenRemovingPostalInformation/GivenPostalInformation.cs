namespace PostalRegistry.Tests.AggregateTests.WhenRemovingPostalInformation
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using PostalInformation;
    using PostalInformation.Commands;
    using PostalInformation.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPostalInformation : PostalRegistryTest
    {
        private readonly Fixture _fixture;

        public GivenPostalInformation(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedPostalCode());
            _fixture.Customize(new WithIntegerNisCode());
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenPostalInformationWasRemoved()
        {
            var command = _fixture.Create<DeletePostalInformation>();

            Assert(
                new Scenario()
                    .Given(command.PostalCode, _fixture.Create<PostalInformationWasRegistered>())
                    .When(command)
                    .Then(new Fact(command.PostalCode, new PostalInformationWasRemoved(command.PostalCode))));
        }

        [Fact]
        public void StateCheck()
        {
            var postalInformationWasRemoved = _fixture.Create<PostalInformationWasRemoved>();

            var sut = PostalInformation.Factory();
            sut.Initialize(new object[]
            {
                _fixture.Create<PostalInformationWasRegistered>(),
                postalInformationWasRemoved
            });

            Xunit.Assert.Empty(sut.PostalNames);
            Xunit.Assert.Null(sut.NisCode);
        }
    }
}
