namespace PostalRegistry.Tests.Import.RelinkMunicipality
{
    using System.Linq;
    using Api.Import.Relink;
    using FluentAssertions;
    using Xunit;

    public sealed class RelinkMunicipalityValidatorTests
    {
        private readonly RelinkMunicipalityRequestValidator _validator;

        public RelinkMunicipalityValidatorTests()
        {
            _validator = new RelinkMunicipalityRequestValidator();
        }

        [Fact]
        public void When_postal_code_is_empty_then_validation_fails()
        {
            var request = new RelinkMunicipalityRequest { PostalCode = string.Empty, NewNisCode = "11111" };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.PostalCode)).Should().HaveCount(1);
        }

        [Fact]
        public void When_nis_code_is_empty_then_validation_fails()
        {
            var request = new RelinkMunicipalityRequest { PostalCode = "9000", NewNisCode = string.Empty };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.NewNisCode)).Should().HaveCount(2);
        }

        [Fact]
        public void When_nis_code_is_not_5_characters_then_validation_fails()
        {
            var request = new RelinkMunicipalityRequest { PostalCode = "9000", NewNisCode = "112" };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Where(x => x.PropertyName == nameof(request.NewNisCode)).Should().HaveCount(1);
        }
    }
}
