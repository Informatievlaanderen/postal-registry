namespace PostalRegistry.Api.Import.UpdatePostalNames
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using FluentValidation;

    public sealed class UpdatePostalNamesRequestValidator : AbstractValidator<UpdatePostalNamesRequest>
    {
        public UpdatePostalNamesRequestValidator()
        {
            RuleFor(request => request.PostalCode)
                .NotEmpty();

            RuleFor(request => request.PostalNamesToAdd.Concat(request.PostalNamesToRemove))
                .NotEmpty()
                .OverridePropertyName(nameof(UpdatePostalNamesRequest.PostalNamesToAdd));

            RuleFor(request => request.PostalNamesToAdd)
                .Must(x =>
                {
                    return x.Select(y => (y.GeografischeNaam.Spelling.ToLower(), y.GeografischeNaam.Taal))
                        .Distinct()
                        .Count() == x.Count;
                });

            RuleForEach(x => x.PostalNamesToAdd)
                .SetValidator(new PostnaamValidator());

            RuleForEach(x => x.PostalNamesToRemove)
                .SetValidator(new PostnaamValidator());
        }
    }

    public sealed class PostnaamValidator : AbstractValidator<Postnaam>
    {
        public PostnaamValidator()
        {
            RuleFor(request => request.GeografischeNaam)
                .NotNull();

            RuleFor(request => request.GeografischeNaam.Spelling)
                .NotEmpty();
        }
    }
}
