namespace PostalRegistry.Tests.Integration
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Options;
    using PostalInformation.Events;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Xunit;

    public class PostalLatestItemTest : IntegrationProjectionTest<PostalLatestItemProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/postinfo";
        private readonly Fixture _fixture;

        public PostalLatestItemTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithIntegerNisCode());
            _fixture.Customize(new WithFixedPostalCode());
        }

        [Fact]
        public async Task WhenPostalInformationWasRegistered()
        {
            var postalInformationWasRegistered = _fixture.Create<PostalInformationWasRegistered>();

            await Sut
                .Given(postalInformationWasRegistered)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationWasRegistered.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Namespace.Should().Be(Namespace);
                    expectedLatestItem.PuriId.Should().Be($"{Namespace}/{postalInformationWasRegistered.PostalCode}");
                    expectedLatestItem.VersionTimestamp.Should()
                        .Be(postalInformationWasRegistered.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenMunicipalityWasAttached()
        {
            var municipalityWasAttached = _fixture.Create<MunicipalityWasAttached>();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    municipalityWasAttached)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(municipalityWasAttached.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.NisCode.Should().Be(municipalityWasAttached.NisCode);
                });
        }

        [Fact]
        public async Task WhenPostalInformationWasRealized()
        {
            var postalInformationWasRealized = _fixture.Create<PostalInformationWasRealized>();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    postalInformationWasRealized)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationWasRealized.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.IsRetired.Should().BeFalse();
                });
        }

        [Fact]
        public async Task WhenPostalInformationPostalNameWasAdded()
        {
            var postalInformationPostalNameWasAdded = new PostalInformationPostalNameWasAddedBuilder(_fixture)
                .WithName("dutchPostal", Language.Dutch)
                .Build();
            var secondPostalNameWasAdded = new PostalInformationPostalNameWasAddedBuilder(_fixture)
                .WithName("dutchFrench", Language.French)
                .Build();

            var thirdPostalNameThatShouldNotBeAdded = new PostalInformationPostalNameWasAddedBuilder(_fixture)
                .WithName(secondPostalNameWasAdded.Name, secondPostalNameWasAdded.Language)
                .Build();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    postalInformationPostalNameWasAdded,
                    secondPostalNameWasAdded,
                    thirdPostalNameThatShouldNotBeAdded)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationPostalNameWasAdded.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Namespace.Should().Be(Namespace);

                    expectedLatestItem.PostalNames.Count.Should().Be(2);

                    expectedLatestItem.IsRetired.Should().BeFalse();
                    expectedLatestItem.PuriId.Should()
                        .Be($"{Namespace}/{postalInformationPostalNameWasAdded.PostalCode}");
                });
        }

        [Fact]
        public async Task WhenPostalInformationPostalNameWasRemoved()
        {
            var postalInformationPostalNameWasAdded = new PostalInformationPostalNameWasAddedBuilder(_fixture)
                .WithName("dutchPostal", Language.Dutch)
                .Build();

            var postalInformationPostalNameWasRemoved = new PostalInformationPostalNameWasRemovedBuilder(_fixture)
                .WithName(postalInformationPostalNameWasAdded.Name, postalInformationPostalNameWasAdded.Language)
                .Build();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    postalInformationPostalNameWasAdded,
                    postalInformationPostalNameWasRemoved)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationPostalNameWasAdded.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.PostalNames.Count.Should().Be(0);
                });
        }

        [Fact]
        public async Task WhenPostalInformationWasRetired()
        {
            var postalInformationWasRetired = _fixture.Create<PostalInformationWasRetired>();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    postalInformationWasRetired)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationWasRetired.PostalCode);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.IsRetired.Should().BeTrue();
                });
        }

        [Fact]
        public async Task WhenPostalInformationWasDeleted()
        {
            var postalInformationWasDeleted = _fixture.Create<PostalInformationWasRemoved>();

            await Sut
                .Given(
                    _fixture.Create<PostalInformationWasRegistered>(),
                    postalInformationWasDeleted)
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.PostalLatestItems.FindAsync(postalInformationWasDeleted.PostalCode);
                    expectedLatestItem.Should().BeNull();
                });
        }

        protected override PostalLatestItemProjections CreateProjection()
            => new PostalLatestItemProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
