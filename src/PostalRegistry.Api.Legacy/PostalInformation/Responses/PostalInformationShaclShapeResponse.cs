namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;

    [DataContract(Name = "PostalInformationShaclShape", Namespace = "")]
    public class PostalInformationShaclShapeReponse
    {
        [DataMember(Name = "@context", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public readonly PostalInformationShaclContext Context = new PostalInformationShaclContext();

        [DataMember(Name = "@id", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public Uri Id { get; set; }

        [DataMember(Name = "@type", Order = 3)]
        [JsonProperty(Required = Required.Always)]
        public readonly string Type = "sh:NodeShape";

        [DataMember(Name = "sh:property", Order = 4)]
        [JsonProperty(Required = Required.Always)]
        public readonly List<PostalInformationShaclProperty> Shape = PostalInformationShaclShape.GetShape();

        public PostalInformationShaclShapeReponse(string apiEndpoint)
        {
            Id = new Uri($"{apiEndpoint}/shape");
        }
    }

    public class PostalInformationShaclShape
    {
        public static List<PostalInformationShaclProperty> GetShape()
            =>  new List<PostalInformationShaclProperty>
            {
                new PostalInformationShaclProperty
                {
                    PropertyPath = "dct:isVersionOf",
                    NodeKind = "sh:IRI",
                    MinimumCount = 1,
                    MaximumCount = 1
                },
                new PostalInformationShaclProperty
                {
                    PropertyPath = "prov:generatedAtTime",
                    DataType = "xsd:dateTime",
                    MinimumCount = 1,
                    MaximumCount = 1
                },
                new PostalInformationShaclProperty
                {
                    PropertyPath = "adms:versionNotes",
                    DataType = "xsd:string",
                    MinimumCount = 1,
                    MaximumCount = 1
                },
                new PostalInformationShaclProperty
                {
                    PropertyPath = "adres:postcode",
                    DataType = "xsd:integer",
                    MinimumCount = 1,
                    MaximumCount = 1
                },
                new PostalInformationShaclProperty
                {
                    PropertyPath = "adres:postnaam",
                    DataType = "rdf:langString"
                },
                new PostalInformationShaclProperty
                {
                    PropertyPath = "br:Postcode.status",
                    DataType = "skos:Concept"
                }
            };
    }

    public class PostalInformationShaclProperty
    {
        [JsonProperty("sh:path")]
        public string PropertyPath { get; set; }

        [JsonProperty("sh:datatype", NullValueHandling = NullValueHandling.Ignore)]
        public string? DataType { get; set; }
        
        [JsonProperty("sh:nodeKind", NullValueHandling = NullValueHandling.Ignore)]
        public string? NodeKind { get; set; }

        [JsonProperty(PropertyName = "sh:minCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinimumCount { get; set; }

        [JsonProperty(PropertyName = "sh:maxCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaximumCount { get; set; }
    }

    public class PostalInformationShaclShapeResponseExamples : IExamplesProvider<PostalInformationShaclShapeReponse>
    {
        private readonly LinkedDataEventStreamOptions _linkedDataEventStreamOptions;
        
        public PostalInformationShaclShapeResponseExamples(IOptions<LinkedDataEventStreamOptions> linkedDataEventStreamOptions)
            => _linkedDataEventStreamOptions = linkedDataEventStreamOptions.Value;

        public PostalInformationShaclShapeReponse GetExamples()
            => new PostalInformationShaclShapeReponse(_linkedDataEventStreamOptions.ApiEndpoint);
    }
}
