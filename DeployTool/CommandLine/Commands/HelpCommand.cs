using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.CommandLine
{
    public class HelpCommand : CliCommand
    {
        public HelpCommand(string name) => this.Name = name;


        public override void Add(CliOption option)
        {
            base.Add(option);
        }

        public override void Validate()
        {
            base.Validate();
        }

        public override string ToString()
        {
            return $"Help";
        }
    }
}
