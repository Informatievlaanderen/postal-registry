namespace PostalRegistry.Tests.Import.UpdatePostalNames
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Import;
    using Api.Import.UpdatePostalNames;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using FluentAssertions;
    using global::AutoFixture;
    using PostalInformation.Commands.BPost;
    using PostalInformation.Commands.Crab;
    using PostalInformation.Events;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class WhenUpdatingPostalNames : ImportApiTest
    {
        private readonly PostalInformationController _controller;
        private readonly Fixture _fixture;

        public WhenUpdatingPostalNames(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateMergerControllerWithUser<PostalInformationController>();
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPostalCode());
        }

        [Fact]
        public void GivenInvalidRequest_ThenValidationErrorIsThrown()
        {
            var act = async () => await _controller.UpdatePostalNames(
                "9000",
                new UpdatePostalNamesRequest(),
                new UpdatePostalNamesRequestValidator(),
                Container.Resolve<IIdempotentCommandHandler>(),
                CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ValidationException>();
        }

        [Fact]
        public void GivenPostalCodeDoesNotExist_ThenApiExceptionIsThrown()
        {
            var act = async () =>
                await _controller.UpdatePostalNames(
                    "9000",
                    new UpdatePostalNamesRequest { PostalNamesToAdd = new List<Postnaam>{_fixture.Create<Postnaam>()}},
                    new UpdatePostalNamesRequestValidator(),
                    Container.Resolve<IIdempotentCommandHandler>(),
                    CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x => x.Message == "Onbestaande postcode");
        }

        [Fact]
        public async Task GivenValidRequest_ThenPostalNamesAreUpdated()
        {
            //Arrange
            _fixture.Register(() => Language.Dutch);
            var importPostalInformationFromBPost = _fixture.Create<ImportPostalInformationFromBPost>();
            DispatchArrangeCommand(importPostalInformationFromBPost, () => importPostalInformationFromBPost.CreateCommandId());

            var importPostalInformationFromCrab = _fixture.Create<ImportPostalInformationFromCrab>()
                .WithSubCantonCode(new CrabSubCantonCode(importPostalInformationFromBPost.PostalCode));
            DispatchArrangeCommand(importPostalInformationFromCrab, () => importPostalInformationFromCrab.CreateCommandId());

            var updatePostalNamesRequest = new UpdatePostalNamesRequest
            {
                PostalCode = importPostalInformationFromCrab.PostalCode,
                PostalNamesToRemove = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam(importPostalInformationFromBPost.PostalNames[0].Name, Taal.NL))
                },
                PostalNamesToAdd = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam("Ghent", Taal.FR))
                }
            };

            //Act
            await _controller.UpdatePostalNames(
                importPostalInformationFromCrab.PostalCode,
                updatePostalNamesRequest,
                new UpdatePostalNamesRequestValidator(),
                Container.Resolve<IIdempotentCommandHandler>(),
                CancellationToken.None);

            //Assert
            var streamStore = Container.Resolve<IStreamStore>();

            var newMessages = await streamStore.ReadStreamBackwards(new StreamId(importPostalInformationFromBPost.PostalCode), 9, 2);
            newMessages.Messages.Length.Should().Be(2);
            var messages = newMessages.Messages.Reverse().ToList();
            messages[0].Type.Should().Be(nameof(PostalInformationPostalNameWasRemoved));
            messages[1].Type.Should().Be(nameof(PostalInformationPostalNameWasAdded));
        }
    }
}
