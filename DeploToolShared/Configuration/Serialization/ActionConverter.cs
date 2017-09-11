using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeployTool.Configuration
{
    public class ActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IAction).IsAssignableFrom(objectType);
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            if (!jo.TryGetValue("ActionName", out JToken actionNameToken))
            {
                throw new JsonSerializationException("Invalid Action");
            }

            var actionName = actionNameToken.ToString();
            switch (actionName)
            {
                case "DotnetPublishAction":
                    return jo.ToObject<DotnetPublishAction>();

                case "SshCopyToRemoteAction":
                    return jo.ToObject<SshCopyToRemoteAction>();

                case "SshRunCommandAction":
                    return jo.ToObject<SshRunCommandAction>();

                case "SshRunAppAction":
                    return jo.ToObject<SshRunAppAction>();

                default:
                    throw new Exception($"Invalid action named {actionName}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
