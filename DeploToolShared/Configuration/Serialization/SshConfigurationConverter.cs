using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeployTool.Configuration
{
    public class SshConfigurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SshConfiguration);
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SshConfiguration item = new SshConfiguration();
            JObject jo = JObject.Load(reader);

            jo.TryRead<string>(serializer, "Host", x => item.Host = x, null);
            jo.TryRead<int>(serializer, "Port", x => item.Port = x, () => item.Port = 22);
            jo.TryRead<string>(serializer, "Username", x => item.Username = x);
            jo.TryRead<string>(serializer, "Password", x => item.Password = x);
            jo.TryRead<string>(serializer, "EncryptedPassword", x => item.EncryptedPassword = x);

            jo.TryRead<PrivateKeyData[]>(serializer, "PrivateKeys", x => item.PrivateKeys = x, () => item.PrivateKeys = new PrivateKeyData[0]);

            jo.TryRead<string>(serializer, "ProxyHost", x => item.ProxyHost = x, null);
            jo.TryRead<int>(serializer, "ProxyPort", x => item.ProxyPort = x, () => item.ProxyPort = 8080);
            jo.TryRead<string>(serializer, "ProxyUsername", x => item.ProxyUsername = x);
            jo.TryRead<string>(serializer, "ProxyPassword", x => item.ProxyPassword = x);
            jo.TryRead<string>(serializer, "EncryptedProxyPassword", x => item.EncryptedProxyPassword = x);

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = value as SshConfiguration;
            if (item == null)
            {
                return;
            }

            var jo = new JObject();
            jo.AddIfNotEqual(serializer, "Host", item.Host, null);
            if (item.Port != 22 && item.Port != 0)
            {
                jo.AddIfNotEqual(serializer, "Port", item.Port, 22);
            }

            jo.AddIfNotEqual(serializer, "Username", item.Username, null);
            jo.AddIfNotEqual(serializer, "Password", item.Password, null);
            jo.AddIfNotEqual(serializer, "EncryptedPassword", item.Password, null);

            jo.AddIfNotEqual(serializer, "PrivateKeys", item.PrivateKeys, null);

            if (item.ProxyHost != null && item.ProxyPort != 0)
            {
                jo.AddIfNotEqual(serializer, "ProxyHost", item.ProxyHost, null);
                jo.AddIfNotEqual(serializer, "ProxyPort", item.ProxyPort, 8080);
                jo.AddIfNotEqual(serializer, "ProxyUsername", item.ProxyUsername, null);
                jo.AddIfNotEqual(serializer, "ProxyPassword", item.ProxyPassword, null);
                jo.AddIfNotEqual(serializer, "EncryptedProxyPassword", item.ProxyPassword, null);
            }

            jo.WriteTo(writer);
        }

    }
}
