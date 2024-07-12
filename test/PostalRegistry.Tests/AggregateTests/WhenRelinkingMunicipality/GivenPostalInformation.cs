namespace PostalRegistry.Tests.AggregateTests.WhenRelinkingMunicipality
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using PostalInformation;
    using PostalInformation.Commands;
    using PostalInformation.Events;
    using PostalInformation.Exceptions;
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
        public void WithInvalidNewNisCode_ThenInvalidNisCodeExceptionIsThrown()
        {
            var command = new RelinkMunicipality(_fixture.Create<PostalCode>(), null, _fixture.Create<Provenance>());

            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>())
                    .When(command)
                    .Throws(new InvalidNisCodeException(null)));
        }

        [Fact]
        public void WithNoNisCodeAttached_ThenMunicipalityWasAttached()
        {
            var command = _fixture.Create<RelinkMunicipality>();

            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>())
                    .When(command)
                    .Then(new Fact(command.PostalCode,
                        new MunicipalityWasAttached(command.PostalCode, command.NewNisCode))));
        }

        [Fact]
        public void WithNisCodeAlreadyAttached_ThenNone()
        {
            var command = _fixture.Create<RelinkMunicipality>();

            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>(),
                        _fixture.Create<MunicipalityWasAttached>())
                    .When(command)
                    .ThenNone());
        }

        [Fact]
        public void ThenMunicipalityWasRelinked()
        {
            var command = new RelinkMunicipality(_fixture.Create<PostalCode>(), new NisCode("12345"), _fixture.Create<Provenance>());

            var municipalityWasAttached = _fixture.Create<MunicipalityWasAttached>();
            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>(),
                        municipalityWasAttached)
                    .When(command)
                    .Then(new Fact(command.PostalCode,
                            new MunicipalityWasRelinked(command.PostalCode, command.NewNisCode, new NisCode(municipalityWasAttached.NisCode)))));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var municipalityWasRelinked = new MunicipalityWasRelinked(
                _fixture.Create<PostalCode>(),
                new NisCode(_fixture.Create<int>().ToString("00000")),
                _fixture.Create<NisCode>());

            // Act
            var sut = PostalInformation.Factory();
            sut.Initialize(new object[]
            {
                _fixture.Create<PostalInformationWasRegistered>(),
                _fixture.Create<MunicipalityWasAttached>(),
                municipalityWasRelinked
            });

            // Assert
            sut.NisCode.Should().Be(new NisCode(municipalityWasRelinked.NewNisCode));
        }
    }
}
