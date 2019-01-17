namespace PostalRegistry.Api.CrabImport.BPostImport
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

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("bpostimport")]
    [ApiExplorerSettings(GroupName = "BPost Import")]
    public class BPostImportController : ApiBusController
    {
        public BPostImportController(ICommandHandlerResolver bus) : base(bus) { }

        /// <summary>
        /// Import een bpost item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandId">Optionele unieke id voor het verzoek.</param>
        /// <param name="registerBPostImport"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het verzoek aanvaard is.</response>
        /// <response code="400">Als het verzoek ongeldige data bevat.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RegisterBPostImportRequestExample), typeof(RegisterBPostImportRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status202Accepted, typeof(RegisterBPostImportResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Post(
            [FromServices] IdempotencyContext context,
            [FromCommandId] Guid commandId,
            [FromBody] RegisterBPostImportRequest registerBPostImport,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // TODO: Check what this returns in the response

            return await IdempotentCommandHandlerDispatch(
                context,
                commandId,
                () => RegisterBPostImportRequestMapping.Map(registerBPostImport),
                cancellationToken);
        }
    }

    public class RegisterBPostImportResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new { };
    }
}
