namespace PostalRegistry.Api.Import
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using UpdatePostalNames;

    public sealed partial class PostalInformationController
    {
        [HttpPost("{postcode}/update-names")]
        public async Task<IActionResult> UpdatePostalNames(
            [FromRoute(Name = "postcode")]string? postalCode,
            [FromBody] UpdatePostalNamesRequest request,
            [FromServices] IValidator<UpdatePostalNamesRequest> validator,
            [FromServices] IIdempotentCommandHandler idempotentCommandHandler,
            CancellationToken cancellationToken = default)
        {
            request.PostalCode = postalCode;
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken);

            try
            {
                var command = new PostalInformation.Commands.UpdatePostalNames(
                    new PostalCode(postalCode!)
                    , request.PostalNamesToAdd.Select(MapPostalName)
                    , request.PostalNamesToRemove.Select(MapPostalName)
                    , CreateProvenance(request.Reason ?? string.Empty));

                await idempotentCommandHandler.Dispatch(
                    command.CreateCommandId(),
                    command,
                    new Dictionary<string, object>(),
                    cancellationToken);

                return Ok();
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException("Onbestaande postcode", StatusCodes.Status404NotFound);
            }
        }

        private static PostalName MapPostalName(Postnaam postnaam)
        {
            switch (postnaam.GeografischeNaam.Taal)
            {
                case Taal.NL:
                    return new PostalName(postnaam.GeografischeNaam.Spelling, Language.Dutch);
                case Taal.FR:
                    return new PostalName(postnaam.GeografischeNaam.Spelling, Language.French);
                case Taal.DE:
                    return new PostalName(postnaam.GeografischeNaam.Spelling, Language.German);
                case Taal.EN:
                    return new PostalName(postnaam.GeografischeNaam.Spelling, Language.English);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
