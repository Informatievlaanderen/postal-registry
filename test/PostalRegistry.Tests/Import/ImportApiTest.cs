namespace PostalRegistry.Tests.Import
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class ImportApiTest : PostalRegistryTest
    {
        protected static JsonSerializerSettings EventsJsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        public ImportApiTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        protected T CreateMergerControllerWithUser<T>() where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T)) as T;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", "Username"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            if (controller != null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

                return controller;
            }

            throw new Exception("Could not find controller type");
        }
    }
}
