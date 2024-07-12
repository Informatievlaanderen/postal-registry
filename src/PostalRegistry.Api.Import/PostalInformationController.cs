namespace PostalRegistry.Api.Import
{
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("import")]
    [ApiExplorerSettings(GroupName = "Import")]
    public sealed partial class PostalInformationController : ApiController
    {
        private Provenance CreateProvenance(string reason, Modification modification = Modification.Update)
        {
            return new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                Application.PostalRegistry,
                new Reason(reason),
                new Operator("OVO002949"),
                modification,
                Organisation.DigitaalVlaanderen);
        }
    }
}
