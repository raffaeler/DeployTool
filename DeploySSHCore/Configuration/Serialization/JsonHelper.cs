using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Helpers;
using Newtonsoft.Json;

namespace DeploySSH.Configuration
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerSettings Settings;
        public static readonly JsonSerializerSettings SettingsWithDefaultValue;

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

            SettingsWithDefaultValue = new JsonSerializerSettings()
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
                DefaultValueHandling = DefaultValueHandling.Populate,
            };
        }

        public static string Serialize(DeployConfiguration deployConfiguration, bool includeDefaults)
        {
            return JsonConvert.SerializeObject(deployConfiguration, 
                includeDefaults ? SettingsWithDefaultValue : Settings);
        }

        public static DeployConfiguration Deserialize(string serialization)
        {
            return JsonConvert.DeserializeObject<DeployConfiguration>(serialization, Settings);
        }
    }
}
