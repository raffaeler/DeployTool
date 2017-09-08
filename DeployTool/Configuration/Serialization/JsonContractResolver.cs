using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace DeployTool.Configuration
{
    public class JsonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type type)
        {
            JsonContract contract = base.CreateContract(type);
            if (typeof(IAction).IsAssignableFrom(type))
            {
                contract.Converter = new ActionConverter();
            }

            return contract;
        }
    }

    //public class ActionJsonContract : JsonContract
    //{
    //    public ActionJsonContract()
    //        : base()
    //    {
    //    }


    //}

    public class CreationConverter<T> : Newtonsoft.Json.Converters.CustomCreationConverter<T>
    {
        public override T Create(Type objectType)
        {
            throw new NotImplementedException();
        }

    }
}
