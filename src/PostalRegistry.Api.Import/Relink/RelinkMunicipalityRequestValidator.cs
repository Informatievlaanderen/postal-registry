namespace PostalRegistry.Api.Import.Relink
{
    using FluentValidation;

    public sealed class RelinkMunicipalityRequestValidator : AbstractValidator<RelinkMunicipalityRequest>
    {
        public RelinkMunicipalityRequestValidator()
        {
            RuleFor(request => request.PostalCode)
                .NotEmpty();

            RuleFor(request => request.NewNisCode)
                .NotEmpty()
                .Length(5);
        }
    }
}
