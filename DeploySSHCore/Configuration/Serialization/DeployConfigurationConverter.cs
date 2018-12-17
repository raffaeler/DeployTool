using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeploySSH.Configuration
{
    public class DeployConfigurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DeployConfiguration);
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DeployConfiguration item = new DeployConfiguration();
            JObject jo = JObject.Load(reader);

            jo.TryRead<string>(serializer, "Description", x => item.Description = x, null);
            jo.TryRead<SshConfiguration>(serializer, "Ssh", x => item.Ssh = x, () => item.Ssh = new SshConfiguration());
            //jo.TryRead<DotnetPublishAction>(serializer, "DotnetPublish", x => item.DotnetPublish = x, () => item.DotnetPublish = new DotnetPublishAction());
            jo.TryRead<IList<IAction>>(serializer, "Actions", x => item.Actions = x, () => item.Actions = new List<IAction>());

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = value as DeployConfiguration;
            if(item == null)
            {
                return;
            }

            var jo = new JObject();
            jo.AddIfNotEqual(serializer, "Description", item.Description, null);
            jo.AddIfNotEqual(serializer, "Ssh", item.Ssh, null);
            //jo.AddIfNotEqual(serializer, "DotNetPublish", item.DotnetPublish, null);
            jo.AddIfNotEqual(serializer, "Actions", item.Actions, null);
            jo.WriteTo(writer);
        }
    }
}
