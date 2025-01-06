namespace PostalRegistry.Tests.AggregateTests.WhenUpdatingPostalNames
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
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
        public void GivenNameAlreadyExists_ThenPostalNameAlreadyExistsExceptionIsThrown()
        {
            var named = _fixture.Create<PostalInformationPostalNameWasAdded>();
            var postalName = new PostalName(named.Name, named.Language);
            var command = new UpdatePostalNames(_fixture.Create<PostalCode>(),
                [postalName],
                new List<PostalName>(),
                _fixture.Create<Provenance>());

            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>(),
                        named)
                    .When(command)
                    .Throws(new PostalNameAlreadyExistsException(postalName)));
        }

        [Fact]
        public void ThenPostalNamesAreUpdated()
        {
            var named = _fixture.Create<PostalInformationPostalNameWasAdded>();
            var nameToAdd = new PostalName(_fixture.Create<string>(), Language.Dutch);
            var command = new UpdatePostalNames(_fixture.Create<PostalCode>(),
                [nameToAdd],
                [new PostalName(named.Name, named.Language)],
                _fixture.Create<Provenance>());

            Assert(
                new Scenario()
                    .Given(
                        command.PostalCode,
                        _fixture.Create<PostalInformationWasRegistered>(),
                        named)
                    .When(command)
                    .Then(new Fact(command.PostalCode,
                            new PostalInformationPostalNameWasRemoved(command.PostalCode, new PostalName(named.Name, named.Language))),
                        new Fact(command.PostalCode,
                            new PostalInformationPostalNameWasAdded(command.PostalCode, nameToAdd))));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var postalInformationPostalNameWasAdded = _fixture.Create<PostalInformationPostalNameWasAdded>();
            var postalInformationPostalNameWasRemoved = new PostalInformationPostalNameWasRemovedBuilder(_fixture)
                .WithName(postalInformationPostalNameWasAdded.Name, postalInformationPostalNameWasAdded.Language)
                .Build();
            var postalInformationPostalNameWasAdded2 = _fixture.Create<PostalInformationPostalNameWasAdded>();

            // Act
            var sut = PostalInformation.Factory();
            sut.Initialize([
                _fixture.Create<PostalInformationWasRegistered>(),
                _fixture.Create<MunicipalityWasAttached>(),
                postalInformationPostalNameWasAdded,
                postalInformationPostalNameWasRemoved,
                postalInformationPostalNameWasAdded2
            ]);

            // Assert
            sut.PostalNames.Count.Should().Be(1);
            sut.PostalNames.Should().Contain(x => x.Name == postalInformationPostalNameWasAdded2.Name && x.Language == postalInformationPostalNameWasAdded2.Language);
        }
    }
}
