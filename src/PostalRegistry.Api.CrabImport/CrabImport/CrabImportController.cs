namespace PostalRegistry.Api.CrabImport.CrabImport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Requests;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabimport")]
    [ApiExplorerSettings(GroupName = "CRAB Import")]
    public class CrabImportController : ApiBusController
    {
        public CrabImportController(ICommandHandlerResolver bus) : base(bus) { }

        /// <summary>
        /// Import een CRAB item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandId">Optionele unieke id voor het verzoek.</param>
        /// <param name="registerCrabImport"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het verzoek aanvaard is.</response>
        /// <response code="400">Als het verzoek ongeldige data bevat.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RegisterCrabImportRequest), typeof(RegisterCrabImportRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status202Accepted, typeof(RegisterCrabImportResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Post(
            [FromServices] IdempotencyContext context,
            [FromCommandId] Guid commandId,
            [FromBody] RegisterCrabImportRequest registerCrabImport,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // TODO: Check what this returns in the response

            return await IdempotentCommandHandlerDispatch(
                context,
                commandId,
                () => RegisterCrabImportRequestMapping.Map(registerCrabImport),
                cancellationToken);
        }
    }

    public class RegisterCrabImportResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new { };
    }
}
