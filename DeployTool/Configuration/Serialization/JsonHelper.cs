using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Helpers;
using Newtonsoft.Json;

namespace DeployTool.Configuration
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerSettings Settings;

        static JsonHelper()
        {
            Settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                //ContractResolver = new JsonContractResolver(),
                Converters = new List<JsonConverter>()
                {
                    new DeployConfigurationConverter(),
                    new SshConfigurationConverter(),
                    new ActionConverter(),
                },

                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };
        }

        public static string Serialize(DeployConfiguration deployConfiguration)
        {
            return JsonConvert.SerializeObject(deployConfiguration, Settings);
        }

        public static DeployConfiguration Deserialize(string serialization)
        {
            return JsonConvert.DeserializeObject<DeployConfiguration>(serialization, Settings);
        }
    }
}
