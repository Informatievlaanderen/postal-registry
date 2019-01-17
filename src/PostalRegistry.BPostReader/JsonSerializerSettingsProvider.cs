namespace PostalRegistry.BPostReader
{
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;

    /// <summary>
    /// Helper class which provides <see cref="JsonSerializerSettings"/>.
    /// </summary>
    public static class JsonSerializerSettingsProvider
    {
        private const int DefaultMaxDepth = 32;

        /// <summary>
        /// Creates default <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <returns>Default <see cref="JsonSerializerSettings"/>.</returns>
        public static JsonSerializerSettings CreateSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new VbrContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },

                MissingMemberHandling = MissingMemberHandling.Ignore,

                // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
                // from deserialization errors that might occur from deeply nested objects.
                MaxDepth = DefaultMaxDepth,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None,
            };
        }
    }

    public class VbrContractResolver : DefaultContractResolver
    {
        public bool SetStringDefaultValueToEmptyString { get; set; }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (prop.PropertyType == typeof(string) && SetStringDefaultValueToEmptyString)
                prop.DefaultValue = "";

            return prop;
        }
    }

    public static class JsonSerializerSettingsExtensions
    {
        /// <summary>
        /// Sets up and adds additional converters for PostalRegister to the JsonSerializerSettings
        /// </summary>
        /// <param name="source"></param>
        /// <returns>the updated JsonSerializerSettings</returns>
        public static JsonSerializerSettings ConfigureForPostalRegistry(this JsonSerializerSettings source)
        {
            var jsonSerializerSettings = JsonSerializerSettingsProvider.CreateSerializerSettings();

            source.ContractResolver = jsonSerializerSettings.ContractResolver;

            if (source.ContractResolver is DefaultContractResolver resolver)
                resolver.NamingStrategy.ProcessDictionaryKeys = true;

            source.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            source.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            //source.DateFormatString = "yyyy-MM-dd";
            //source.Converters.Add(new TrimStringConverter());
            //source.Converters.Add(new GuidConverter());

            return source
                .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                .WithIsoIntervalConverter();
        }
    }
}
