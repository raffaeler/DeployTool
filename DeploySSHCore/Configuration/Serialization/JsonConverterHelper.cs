using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeploySSH.Configuration
{
    public static class JsonConverterHelper
    {
        public static void TryRead<T>(this JObject jo, JsonSerializer serializer, string propertyName,
            Action<T> assignValue, Action assignDefault = null)
        {
            if (jo.TryGetValue(propertyName, out JToken token))
            {
                assignValue(token.ToObject<T>(serializer));
            }
            else
            {
                if (assignDefault != null)
                    assignDefault();
            }
        }

        public static void AddIfNotEqual<T>(this JObject jo, JsonSerializer serializer, string propertyName,
            T current, T defaultValue)
        {
            if (Equals(current, defaultValue))
            {
                return;
            }

            jo.Add(propertyName, JToken.FromObject(current, serializer));
        }
    }
}
