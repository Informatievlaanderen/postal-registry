namespace PostalRegistry.Api.CrabImport.BPostImport.Requests
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using PostalInformation;
    using Swashbuckle.AspNetCore.Filters;

    public class RegisterBPostImportRequest
    {
        /// <summary>Type van het bpost item.</summary>
        [Required]
        public string Type { get; set; }

        /// <summary>Het bpost item.</summary>
        [Required]
        public string BPostItem { get; set; }
    }

    public class RegisterBPostImportRequestExample : IExamplesProvider
    {
        public object GetExamples()
            => new RegisterBPostImportRequest
            {
                Type = "PostalRegistry.PostalInformation.Commands.BPost.ImportPostalInformationFromBPost",
                BPostItem = "{}"
            };
    }

    public static class RegisterBPostImportRequestMapping
    {
        public static dynamic Map(RegisterBPostImportRequest message)
        {
            var assembly = typeof(PostalInformation).Assembly;
            var type = assembly.GetType(message.Type);

            return JsonConvert.DeserializeObject(message.BPostItem, type);
        }
    }
}
