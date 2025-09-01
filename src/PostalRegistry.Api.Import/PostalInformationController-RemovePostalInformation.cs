namespace PostalRegistry.Api.Import
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using PostalInformation.Commands;

    public sealed partial class PostalInformationController
    {
        [HttpDelete("{postcode}")]
        public async Task<IActionResult> RemovePostalInformation(
            [FromRoute(Name = "postcode")]string? postalCode,
            [FromServices] IIdempotentCommandHandler idempotentCommandHandler,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new DeletePostalInformation(
                    new PostalCode(postalCode!)
                    , CreateProvenance("Verwijder postinformatie"));

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
