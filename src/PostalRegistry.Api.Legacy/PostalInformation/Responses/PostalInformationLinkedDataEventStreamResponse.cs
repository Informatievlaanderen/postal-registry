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
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;

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
        public readonly string Type = "tree:Node";

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
        private readonly string _apiEndPoint;
        private readonly string _dataVlaanderenNamespace;

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
        public List<LanguageValue>? PostalNames { get; set; }

        [DataMember(Name = "status", Order = 7)]
        [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Status { get; set; }

        public PostalInformationVersionObject(
            string apiEndPoint,
            string dataVlaanderenNamespace,
            string objectIdentifier,
            string changeType,
            Instant generatedAtTime,
            string postalCode,
            IEnumerable<PostalName>? postalNames,
            PostalInformationStatus? status)
        {
            _apiEndPoint = apiEndPoint;
            _dataVlaanderenNamespace = dataVlaanderenNamespace;
            ChangeType = changeType;
            PostalCode = postalCode;
            GeneratedAtTime = generatedAtTime.ToBelgianDateTimeOffset();

            Id = CreateVersionUri(objectIdentifier);
            IsVersionOf = GetPersistentUri(PostalCode);
            PostalNames = TransformPostalNames(postalNames);
            Status = GetStatusUri(status);
        }

        private Uri CreateVersionUri(string identifier) => new Uri($"{_apiEndPoint}#{identifier}");

        private Uri GetPersistentUri(string id) => new Uri($"{_dataVlaanderenNamespace}/{id}");

        private List<LanguageValue> TransformPostalNames(IEnumerable<PostalName> postalNames)
        {
            if (postalNames.ToList().Count == 0)
                return null;

            return postalNames.Select(postalname => new LanguageValue { Value = postalname.Name, Language = GetLanguageIdentifier(postalname.Language) }).ToList();
        }

        private Uri GetStatusUri(PostalInformationStatus? status)
        {
            switch (status)
            {
                case PostalInformationStatus.Current:
                    return new Uri("https://data.vlaanderen.be/id/concept/binairestatus/gerealiseerd");

                case PostalInformationStatus.Retired:
                    return new Uri("https://data.vlaanderen.be/id/concept/binairestatus/gehistoreerd");

                default:
                    return null;
            }
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

    public class LanguageValue
    {
        [JsonProperty("@value")]
        public string Value { get; set; }

        [JsonProperty("@language")]
        public string Language { get; set; }
    }

    public class PostalInformationLinkedDataEventStreamResponseExamples : IExamplesProvider<PostalInformationLinkedDataEventStreamResponse>
    {
        private readonly string _apiEndPoint;
        private readonly string _dataVlaanderenNamespace;

        public PostalInformationLinkedDataEventStreamResponseExamples(
            IOptions<LinkedDataEventStreamOptions> linkedDataOptions,
            IOptions<ResponseOptions> responseOptions)
        {
            _apiEndPoint = linkedDataOptions.Value.ApiEndpoint;
            _dataVlaanderenNamespace = responseOptions.Value.Naamruimte;
        }

        public PostalInformationLinkedDataEventStreamResponse GetExamples()
        {
            var postalNames = new List<PostalName>()
            {
                new PostalName("Gent", Language.Dutch)
            };

            var generatedAtTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse("2019-11-21 08:34:51.6721003 +00:00"));
            var versionObjects = new List<PostalInformationVersionObject>()
            {
                new PostalInformationVersionObject(
                    _apiEndPoint,
                    _dataVlaanderenNamespace,
                    "42C1E3C14343FF85314CDB75759978C6",
                    "PostalInformationPostalNameWasAdded",
                    generatedAtTime,
                    "9000",
                    postalNames,
                    PostalInformationStatus.Current)
            };
            var hypermediaControls = new List<HypermediaControl>()
            {
                new HypermediaControl
                {
                    Type = "tree:Relation",
                    Node = new Uri("https://data.vlaanderen.be/base/postinfo?page=2")
                }
            };

            return new PostalInformationLinkedDataEventStreamResponse
            {
                Context = new PostalInformationLinkedDataContext(),
                Id = new Uri("https://data.vlaanderen.be/base/postinfo?page=1"),
                CollectionLink = new Uri("https://data.vlaanderen/base/postinfo"),
                HypermediaControls = hypermediaControls,
                PostalInformationShape = new Uri("https://data.vlaanderen.be/base/postinfo/shape"),
                Items = versionObjects
            };
        }
    }
}
