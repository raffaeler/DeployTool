using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.CommandLine
{
    public class InteractCommand : CliCommand
    {
        public InteractCommand(string name) => this.Name = name;

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
            return $"Interact";
        }
    }

}
