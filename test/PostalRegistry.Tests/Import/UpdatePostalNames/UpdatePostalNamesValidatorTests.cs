namespace PostalRegistry.Tests.Import.UpdatePostalNames
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Import.UpdatePostalNames;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using FluentAssertions;
    using Xunit;

    public sealed class UpdatePostalNamesValidatorTests
    {
        private readonly UpdatePostalNamesRequestValidator _validator;

        public UpdatePostalNamesValidatorTests()
        {
            _validator = new UpdatePostalNamesRequestValidator();
        }

        [Fact]
        public void When_postal_code_is_empty_then_validation_fails()
        {
            var request = new UpdatePostalNamesRequest { PostalCode = string.Empty };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.PostalCode)).Should().HaveCount(1);
        }

        [Fact]
        public void When_postal_names_to_add_and_remove_are_empty_then_validation_fails()
        {
            var request = new UpdatePostalNamesRequest { PostalCode = "9000" };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.PostalNamesToAdd)).Should().HaveCount(1);
        }

        [Fact]
        public void When_postal_names_to_add_contains_duplicates_then_validation_fails()
        {
            var request = new UpdatePostalNamesRequest
            {
                PostalCode = "9000",
                PostalNamesToAdd = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam ("Gent", Taal.NL)),
                    new Postnaam(new GeografischeNaam ("Gent", Taal.NL))
                }
            };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.PostalNamesToAdd)).Should().HaveCount(1);
        }

        [Fact]
        public void When_postal_name_has_empty_spelling_then_validation_fails()
        {
            var request = new UpdatePostalNamesRequest
            {
                PostalCode = "9000",
                PostalNamesToAdd = new List<Postnaam>
                {
                    new Postnaam(new GeografischeNaam (string.Empty, Taal.NL))
                }
            };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == $"{nameof(request.PostalNamesToAdd)}[0].GeografischeNaam.Spelling").Should().HaveCount(1);
        }
    }
}
