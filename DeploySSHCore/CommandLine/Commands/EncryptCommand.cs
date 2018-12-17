using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.CommandLine
{
    public class EncryptCommand : CliCommand
    {
        public EncryptCommand(string name) => this.Name = name;

        //public string Data { get; private set; }

        //public bool IsClear { get; set; }

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
            return $"Data encrypted";
        }
    }
}
