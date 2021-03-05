namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;
    using PostalRegistry.Api.Legacy.Infrastructure;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    [DataContract(Name = "PostinfoLinkedDataEventStream", Namespace = "")]
    public class PostalInformationLinkedDataEventStreamResponse
    {
        [DataMember(Name = "@context", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public object Context { get; set; }

        [DataMember(Name = "@id", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public Uri Id { get; set; }

        [DataMember(Name = "@type", Order = 3)]
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [DataMember(Name = "viewOf", Order = 4)]
        [JsonProperty(Required = Required.Always)]
        public Uri CollectionLink { get; set; }

        [DataMember(Name = "tree:shape", Order = 5)]
        [JsonProperty(Required = Required.Always)]
        public Uri PostalInformationShape { get; set; }

        [DataMember(Name = "tree:relation", Order = 6)]
        [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<HypermediaControl>? HypermediaControls { get; set; }

        [DataMember(Name = "items", Order = 7)]
        [JsonProperty(Required = Required.Always)]
        public List<PostalInformationVersionObject> Items { get; set; }
    }

    [DataContract(Name = "PostinfoVersieObject", Namespace = "")]
    public class PostalInformationVersionObject
    {
        [DataMember(Name = "@id", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public Uri Id { get; set; }

        [DataMember(Name = "@type", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public readonly string Type = "Postinfo";

        [DataMember(Name = "isVersionOf", Order = 3)]
        [JsonProperty(Required = Required.Always)]
        public Uri IsVersionOf { get; set; }

        [DataMember(Name = "generatedAtTime", Order = 4)]
        [JsonProperty(Required = Required.Always)]
        public DateTimeOffset GeneratedAtTime { get; set; }

        [DataMember(Name = "eventName", Order = 5)]
        [JsonProperty(Required = Required.Always)]
        public string ChangeType { get; set; }

        [DataMember(Name = "postcode", Order = 6)]
        [JsonProperty(Required = Required.Always)]
        public string PostalCode { get; set; }

        [DataMember(Name = "postnaam", Order = 7)]
        [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<PostalNameTransformed>? PostalNames { get; set; }

        [DataMember(Name = "status", Order = 7)]
        [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Status { get; set; }

        [IgnoreDataMember]
        public LinkedDataEventStreamConfiguration Configuration { get; set; }

        public PostalInformationVersionObject(
            LinkedDataEventStreamConfiguration configuration,
            long position,
            string changeType,
            Instant generatedAtTime,
            string postalCode,
            IEnumerable<PostalName>? postalNames,
            PostalInformationStatus? status)
        {
            Configuration = configuration;
            ChangeType = changeType;
            PostalCode = postalCode;
            GeneratedAtTime = generatedAtTime.ToBelgianDateTimeOffset();

            Id = CreateVersionUri(position);
            IsVersionOf = GetPersistentUri(PostalCode);
            PostalNames = TransformPostalNames(postalNames);
            Status = GetStatusUri(status);
        }

        private Uri CreateVersionUri(long code) => new Uri($"{Configuration.ApiEndpoint}#{code}");

        private Uri GetPersistentUri(string id) => new Uri($"{Configuration.DataVlaanderenNamespace}/{id}");

        private List<PostalNameTransformed> TransformPostalNames(IEnumerable<PostalName> postalNames)
        {
            if (postalNames.ToList().Count == 0)
                return null;

            return postalNames.Select(postalname => new PostalNameTransformed { Name = postalname.Name, Language = GetLanguageIdentifier(postalname.Language) }).ToList();
        }

        private Uri? GetStatusUri(PostalInformationStatus? status)
        {
            Uri statusUri = null;
            switch (status)
            {
                case PostalInformationStatus.Current:
                    statusUri = new Uri("https://data.vlaanderen.be/id/concept/binairestatus/gerealiseerd");
                    break;

                case PostalInformationStatus.Retired:
                    statusUri = new Uri("https://data.vlaanderen.be/id/concept/binairestatus/gehistoreerd");
                    break;
            }

            return statusUri;
        }

        private static string GetLanguageIdentifier(Language language)
        {
            switch (language)
            {
                case Language.Dutch:
                    return "nl";

                case Language.French:
                    return "fr";

                case Language.English:
                    return "en";

                case Language.German:
                    return "de";

                default:
                    throw new Exception("Unable to identify the language.");
            }
        }
    }

    public class PostalNameTransformed
    {
        [JsonProperty("@value")]
        public string Name { get; set; }

        [JsonProperty("@language")]
        public string Language { get; set; }
    }

    public class PostalInformationLinkedDataEventStreamResponseExamples : IExamplesProvider<PostalInformationLinkedDataEventStreamResponse>
    {
        private readonly LinkedDataEventStreamConfiguration _configuration;
        public PostalInformationLinkedDataEventStreamResponseExamples(LinkedDataEventStreamConfiguration configuration) => _configuration = configuration;

        public PostalInformationLinkedDataEventStreamResponse GetExamples()
        {
            var postalNames = new List<PostalName>()
            {
                new PostalName("Gent", Language.Dutch)
            };

            var generatedAtTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse("2002-11-21T11:23:45+01:00"));
            var versionObjects = new List<PostalInformationVersionObject>()
            {
                new PostalInformationVersionObject(
                    _configuration,
                    0,
                    "PostalInformationWasRealized",
                    generatedAtTime,
                    "9000",
                    postalNames,
                    PostalInformationStatus.Current)
            };
            var hypermediaControls = new List<HypermediaControl>()
            {
                new HypermediaControl
                {
                    Type = "tree:GreaterThanOrEqualToRelation",
                    Node = new Uri("https://data.vlaanderen.be/base/postinfo?page=2"),
                    SelectedProperty = "prov:generatedAtTime",
                    TreeValue = new Literal
                    {
                        Value = DateTimeOffset.Parse("2002-11-21T11:23:45+01:00"),
                        Type = "xsd:dateTime"
                    }
                }
            };

            return new PostalInformationLinkedDataEventStreamResponse
            {
                Context = new PostalInformationLinkedDataContext(),
                Id = new Uri("https://data.vlaanderen.be/base/postinfo?page=1"),
                Type = "tree:Node",
                CollectionLink = new Uri("https://data.vlaanderen/base/postinfo"),
                HypermediaControls = hypermediaControls,
                PostalInformationShape = new Uri("https://data.vlaanderen.be/base/postinfo/shape"),
                Items = versionObjects
            };
        }
    }
}
