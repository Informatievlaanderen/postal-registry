using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NodaTime;
using PostalRegistry;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace PostalRegistry.Projections.Legacy.PostalInformationSyndication
{
    public static class JsonLdTools
    {
        private const string BASE_URL = "https://data.vlaanderen.be/base/postinfo";
        private const string BASE_PURI = "https://data.vlaanderen.be/id/postinfo";
        private const string TYPE = "Postinfo";

        //TODO: create has for Id

        public static string ToJsonLd(this PostalInformationSyndicationItem syndicationItem)
        {
            var version = new PostalRegistryVersionObject
            {
                Id = createVersionUri(syndicationItem.Position.ToString()),
                Type = TYPE,
                EventName = syndicationItem.ChangeType,
                IsVersionOf = constructPersistentUri(syndicationItem.PostalCode.ToString()),
                GeneratedAtTime = syndicationItem.RecordCreatedAt,
                PostalCode = Int32.Parse(syndicationItem.PostalCode),
                PostalNames = TransformPostalNames(syndicationItem.PostalNames),
                Status = GetStatusUri(syndicationItem.Status)
            };
            return JsonConvert.SerializeObject(version);
            
        }

        private static Uri createVersionUri(string code)
        {
            return new Uri(BASE_URL + "#" + code);
        }

        private static Uri constructPersistentUri(string id)
        {
            return new Uri(BASE_PURI + "/" + id);
        }

        private static List<PostalNameTransformed> TransformPostalNames(IReadOnlyList<PostalName> postalNames)
        {
            if (postalNames.Count == 0)
            {
                return null;
            }

            List<PostalNameTransformed> postalNamesTransformed = new List<PostalNameTransformed>();
            foreach(var postalName in postalNames)
            {
                postalNamesTransformed.Add(new PostalNameTransformed
                {
                    name = postalName.Name,
                    language = GetLanguageIdentifier(postalName.Language)
                });
            }

            // Results in an error
            //var postalNamesTransformed = (List<PostalNameTransformed>) postalNames.Select(postalname => new PostalNameTransformed { name = postalname.Name, language = GetLanguageIdentifier(postalname.Language)});
            return postalNamesTransformed;
        }

        private static Uri GetStatusUri(PostalInformationStatus? status)
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
            string identifier = "";
            switch (language)
            {
                case Language.Dutch:
                    identifier = "nl";
                    break;
                case Language.French:
                    identifier = "fr";
                    break;
                case Language.English:
                    identifier = "en";
                    break;
                case Language.German:
                    identifier = "de";
                    break;
            }

            if (String.IsNullOrEmpty(identifier))
            {
                throw new Exception("[JsonLd]: unable to identify the language.");
            }

            return identifier;
        }

      
    }

    public class PostalRegistryVersionObject
    {
        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [JsonProperty("isVersionOf")]
        public Uri IsVersionOf { get; set; }

        [JsonProperty("generatedAtTime")]
        public Instant GeneratedAtTime { get; set; }

        [JsonProperty("postcode")]
        public int PostalCode { get; set; }

        [JsonProperty("postnaam", NullValueHandling = NullValueHandling.Ignore)]
        public List<PostalNameTransformed>? PostalNames { get; set; }

        // TODO: check if we have a RDF property to assign to this property
        // If we decide to ignore the status, maybe we can also ignore the event PostalInformationWasRealized?
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Status { get; set; }
    }

    // In this class we transform the current form of postal names to a more json-ld compliant form
    public class PostalNameTransformed
    {
        [JsonProperty("@value")]
        public string name { get; set; }

        [JsonProperty("@language")]
        public string language { get; set; }
    }
}
