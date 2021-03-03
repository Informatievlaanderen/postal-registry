using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    [DataContract(Name="PostinfoLinkedDataEventStream", Namespace="")]
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
        public List<HypermediaControls>? HypermediaControls { get; set; }

        [DataMember(Name = "items", Order = 7)]
        [JsonProperty(Required = Required.Always)]
        public List<PostalInformationVersionObject> Items { get; set; }


    }

    [DataContract(Name="PostinfoVersieObject", Namespace="")]
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

        public PostalInformationVersionObject(
            IConfigurationSection configuration,
            long position,
            string changeType,
            Instant generatedAtTime,
            string postalCode,
            IEnumerable<PostalName>? postalNames,
            PostalInformationStatus? status)
        {

            ChangeType = changeType;
            PostalCode = postalCode;
            GeneratedAtTime = generatedAtTime.ToBelgianDateTimeOffset();

            Id = CreateVersionUri(configuration, position);
            IsVersionOf = GetPersistentUri(configuration, PostalCode);
            PostalNames = TransformPostalNames(postalNames);
            Status = GetStatusUri(status);
        }

        private static Uri CreateVersionUri(IConfigurationSection configuration, long code)
        {
            return new Uri($"{configuration["ApiEndpoint"]}#{code}");
        }

        private static Uri GetPersistentUri(IConfigurationSection configuration, string id)
        {
            return new Uri($"{configuration["DataVlaanderenNamespace"]}/{id}");
        }

        private static List<PostalNameTransformed> TransformPostalNames(IEnumerable<PostalName> postalNames)
        {
            if (postalNames.ToList().Count == 0)
            {
                return null;
            }

            var postalNamesTransformed = postalNames.Select(postalname => new PostalNameTransformed { name = postalname.Name, language = GetLanguageIdentifier(postalname.Language)}).ToList();
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
                throw new Exception("Unable to identify the language.");
            }

            return identifier;
        }


    }

    public class PostalNameTransformed
    {
        [JsonProperty("@value")]
        public string name { get; set; }

        [JsonProperty("@language")]
        public string language { get; set; }
    }

}


