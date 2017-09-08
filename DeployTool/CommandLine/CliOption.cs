using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeployTool
{
    public class CliOption// : ICliOption
    {
        public CliOption()
        {
            RawParameters = new List<string>();
        }

        public string Name { get; set; }

        public IList<string> RawParameters { get; private set; }

        public override string ToString()
        {
            return $"[{Name}:{string.Join(";", RawParameters)}]";
        }

        internal string AssertSingleParameter()
        {
            if (RawParameters.Count != 1)
            {
                throw new ArgumentException($"Expected a single parameter for option {Name}");
            }

            return RawParameters.Single();
        }

        internal void AssertValidName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("A nameless option was encountered");
            }
        }
    }
}
