namespace PostalRegistry.Api.Legacy.Infrastructure
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LinkedDataEventStreamConfiguration
    {
        public string DataVlaanderenNamespace { get; }
        public string ApiEndpoint { get; }

        public LinkedDataEventStreamConfiguration(IConfigurationSection configuration)
        {
            DataVlaanderenNamespace = configuration["DataVlaanderenNamespace"];
            ApiEndpoint = configuration["ApiEndpoint"];
        }
    }
}
