namespace PostalRegistry.Tests.Import.RelinkMunicipality
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Import;
    using Api.Import.Relink;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Crab;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Newtonsoft.Json;
    using PostalInformation.Commands.BPost;
    using PostalInformation.Commands.Crab;
    using PostalInformation.Events;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class WhenRelinkingMunicipality : ImportApiTest
    {
        private readonly PostalInformationController _controller;
        private readonly Fixture _fixture;

        public WhenRelinkingMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateMergerControllerWithUser<PostalInformationController>();
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPostalCode());
            _fixture.Customize(new WithIntegerNisCode());
        }

        [Fact]
        public void GivenInvalidRequest_ThenValidationErrorIsThrown()
        {
            var act = async () => await _controller.RelinkMunicipality(
                "9000",
                new RelinkMunicipalityRequest(),
                new RelinkMunicipalityRequestValidator(),
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
                await _controller.RelinkMunicipality(
                    "9000",
                    new RelinkMunicipalityRequest { NewNisCode = "10001" },
                    new RelinkMunicipalityRequestValidator(),
                    Container.Resolve<IIdempotentCommandHandler>(),
                    CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x => x.Message == "Onbestaande postcode");
        }

        [Fact]
        public async Task GivenValidRequest_ThenMunicipalityIsRelinked()
        {
            var importPostalInformationFromBPost = _fixture.Create<ImportPostalInformationFromBPost>();
            DispatchArrangeCommand(importPostalInformationFromBPost, () => importPostalInformationFromBPost.CreateCommandId());

            var importPostalInformationFromCrab = _fixture.Create<ImportPostalInformationFromCrab>()
                .WithSubCantonCode(new CrabSubCantonCode(importPostalInformationFromBPost.PostalCode));

            DispatchArrangeCommand(importPostalInformationFromCrab, () => importPostalInformationFromCrab.CreateCommandId());

            await _controller.RelinkMunicipality(
                importPostalInformationFromCrab.PostalCode,
                new RelinkMunicipalityRequest { NewNisCode = "10001" },
                new RelinkMunicipalityRequestValidator(),
                Container.Resolve<IIdempotentCommandHandler>(),
                CancellationToken.None);

            var streamStore = Container.Resolve<IStreamStore>();

            var newMessages = await streamStore.ReadStreamBackwards(new StreamId(importPostalInformationFromBPost.PostalCode), 8, 1);
            newMessages.Messages.Length.Should().Be(1);
            newMessages.Messages[0].Type.Should().Be(nameof(MunicipalityWasRelinked));
            var municipalityWasRelinked = JsonConvert.DeserializeObject<MunicipalityWasRelinked>(await newMessages.Messages[0].GetJsonData(), EventsJsonSerializerSettings);
            municipalityWasRelinked.Should().NotBeNull();
            municipalityWasRelinked.NewNisCode.Should().Be("10001");
            municipalityWasRelinked.PreviousNisCode.Should().Be(importPostalInformationFromCrab.NisCode);
        }

        private void DispatchArrangeCommand<T>(T command, Func<Guid> createCommandId)
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(createCommandId(), command).GetAwaiter().GetResult();
        }
    }
}
