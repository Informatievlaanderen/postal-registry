namespace PostalRegistry.Api.Import
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using PostalInformation.Commands;
    using Relink;

    public sealed partial class PostalInformationController
    {
        [HttpPost("{postcode}/relink-municipality")]
        public async Task<IActionResult> RelinkMunicipality(
            [FromRoute(Name = "postcode")]string? postalCode,
            [FromBody] RelinkMunicipalityRequest request,
            [FromServices] IValidator<RelinkMunicipalityRequest> validator,
            [FromServices] IIdempotentCommandHandler idempotentCommandHandler,
            CancellationToken cancellationToken = default)
        {
            request.PostalCode = postalCode;
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken);

            try
            {
                var command = new RelinkMunicipality(
                    new PostalCode(postalCode!)
                    , new NisCode(request.NewNisCode!)
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
    }
}
