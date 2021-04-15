namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;
    using NodaTime;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
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

        public PostalInformationLinkedDataEventStreamResponse(
            string apiEndpoint,
            int page,
            int pageSize,
            List<PostalInformationVersionObject> pagedPostalInformationVersionObjects)
        {
            Context = new PostalInformationLinkedDataContext();
            Id = new Uri($"{apiEndpoint}?page={page}");
            CollectionLink = new Uri($"{apiEndpoint}");
            PostalInformationShape = new Uri($"{apiEndpoint}/shape");
            HypermediaControls = GetHypermediaControls(
                apiEndpoint,
                page,
                pageSize,
                pagedPostalInformationVersionObjects);
            Items = pagedPostalInformationVersionObjects;
        }

        private static List<HypermediaControl> GetHypermediaControls(
            string apiEndpoint,
            int page,
            int pageSize,
            List<PostalInformationVersionObject> items)
        {
            var controls = new List<HypermediaControl>();

            var previous = AddPrevious(apiEndpoint, page);
            if (previous != null)
                controls.Add(previous);

            var next = AddNext(apiEndpoint, page, pageSize, items);
            if (next != null)
                controls.Add(next);

            return controls.Count > 0 ? controls : null;
        }

        private static HypermediaControl AddPrevious(
            string apiEndpoint,
            int page)
        {
            if (page <= 1)
                return null;

            var previousUrl = new Uri($"{apiEndpoint}?page={page - 1}");

            return new HypermediaControl
            {
                Type = "tree:Relation",
                Node = previousUrl
            };
        }

        private static HypermediaControl AddNext(
            string apiEndpoint,
            int page,
            int pageSize,
            List<PostalInformationVersionObject> items)
        {
            if (items.Count != pageSize)
                return null;

            var nextUrl = new Uri($"{apiEndpoint}?page={page + 1}");

            return new HypermediaControl
            {
                Type = "tree:Relation",
                Node = nextUrl
            };
        }
    }

    [DataContract(Name = "PostinfoVersieObject", Namespace = "")]
    public class PostalInformationVersionObject
    {
        private readonly string _apiEndpoint;
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
        public List<LanguageValue> PostalNames { get; set; }

        [DataMember(Name = "status", Order = 7)]
        [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Status { get; set; }

        public PostalInformationVersionObject(
            string apiEndpoint,
            string dataVlaanderenNamespace,
            string objectIdentifier,
            string changeType,
            Instant generatedAtTime,
            string postalCode,
            IEnumerable<PostalName> postalNames,
            PostalInformationStatus? status)
        {
            _apiEndpoint = apiEndpoint;
            _dataVlaanderenNamespace = dataVlaanderenNamespace;
            ChangeType = changeType;
            PostalCode = postalCode;
            GeneratedAtTime = generatedAtTime.ToBelgianDateTimeOffset();

            Id = CreateVersionUri(objectIdentifier);
            IsVersionOf = GetPersistentUri(PostalCode);
            PostalNames = TransformPostalNames(postalNames);
            Status = GetStatusUri(status);
        }

        private Uri CreateVersionUri(string identifier) => new Uri($"{_apiEndpoint}#{identifier}");

        private Uri GetPersistentUri(string id) => new Uri($"{_dataVlaanderenNamespace}/{id}");

        private static List<LanguageValue> TransformPostalNames(IEnumerable<PostalName> postalNames)
        {
            return postalNames
                .Select(postalname =>
                    new LanguageValue
                    {
                        Value = postalname.Name,
                        Language = GetLanguageIdentifier(postalname.Language)
                    }).ToList();
        }

        private static Uri GetStatusUri(PostalInformationStatus? status)
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

    public class HypermediaControl
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("tree:node")]
        public Uri Node { get; set; }
    }

    public class PostalInformationLinkedDataEventStreamResponseExamples : IExamplesProvider<PostalInformationLinkedDataEventStreamResponse>
    {
        private readonly string _apiEndpoint;
        private readonly string _dataVlaanderenNamespace;

        public PostalInformationLinkedDataEventStreamResponseExamples(
            IOptions<LinkedDataEventStreamOptions> linkedDataOptions,
            IOptions<ResponseOptions> responseOptions)
        {
            _apiEndpoint = linkedDataOptions.Value.ApiEndpoint;
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
                    _apiEndpoint,
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

            return new PostalInformationLinkedDataEventStreamResponse(
                _apiEndpoint,
                1,
                100,
                versionObjects)
            {
                HypermediaControls = hypermediaControls,
            };
        }
    }
}
