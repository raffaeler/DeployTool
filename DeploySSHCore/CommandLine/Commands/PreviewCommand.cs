using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.CommandLine
{
    public class PreviewCommand : CliCommand
    {
        public PreviewCommand(string name) => this.Name = name;

        public override void Add(CliOption option)
        {
            base.Add(option);

            option.AssertValidName();
        }

        public override void Validate()
        {
            base.Validate();
        }

        public override string ToString()
        {
            return $"Preview";
        }
    }

}
