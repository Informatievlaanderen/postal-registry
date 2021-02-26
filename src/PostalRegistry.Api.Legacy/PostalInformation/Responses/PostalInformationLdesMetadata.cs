using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    public class PostalInformationLdesMetadata
    {
        public static Uri GetPageIdentifier(IConfiguration configuration, int page)
        {
            return new Uri($"{configuration["ApiEndpoint"]}?page={page}");
        }

        public static Uri GetCollectionLink(IConfiguration configuration)
        {
            return new Uri($"{configuration["ApiEndpoint"]}");
        }

        public static List<HypermediaControls>? GetHypermediaControls(List<PostalInformationVersionObject> items, IConfiguration configuration, int page, int pageSize)
        {
            List<HypermediaControls> controls = new List<HypermediaControls>();

            var previous = AddPrevious(items, configuration, page);
            if(previous != null)
            {
                controls.Add(previous);
            }

            var next = AddNext(items, configuration, page, pageSize);
            if(next != null)
            {
                controls.Add(next);
            }

            return controls.Count > 0 ? controls: null;
        }

        private static HypermediaControls? AddPrevious(List<PostalInformationVersionObject> items, IConfiguration configuration, int page)
        {
            if(page <= 1)
            {
                return null; ;
            }

            var previousUrl = new Uri($"{configuration["ApiEndpoint"]}?page={page - 1}");

            return new HypermediaControls
            {
                Type = "tree:LessThanOrEqualToRelation",
                Node = previousUrl,
                SelectedProperty = "prov:generatedAtTime",
                CompareValue = new CompareValue
                {
                    Value = items.FirstOrDefault().GeneratedAtTime,
                    Type = "xsd:dateTime"
                }
            };
        }

        private static HypermediaControls? AddNext(List<PostalInformationVersionObject> items, IConfiguration configuration, int page, int pageSize)
        {
            if(items.Count != pageSize)
            {
                return null;
            }

            var nextUrl = new Uri($"{configuration["ApiEndpoint"]}?page={page + 1}");

            return new HypermediaControls
            {
                Type = "tree:GreaterThanOrEqualToRelation",
                Node = nextUrl,
                SelectedProperty = "prov:generatedAtTime",
                CompareValue = new CompareValue
                {
                    Value = items[items.Count - 1].GeneratedAtTime,
                    Type = "xsd:dateTime"
                }
            };
        }
    }

    public class HypermediaControls
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("tree:node")]
        public Uri Node { get; set; }

        [JsonProperty("tree:path")]
        public string SelectedProperty { get; set; }

        [JsonProperty("tree:value")]
        public CompareValue CompareValue { get; set; }
    }

    public class CompareValue
    {
        [JsonProperty("@value")]
        public DateTimeOffset Value { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
    }
}
