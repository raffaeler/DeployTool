using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.CommandLine
{
    public class RunCommand : CliCommand
    {
        public RunCommand(string name) => this.Name = name;

        public string Filename { get; private set; }

        public override void Add(CliOption option)
        {
            base.Add(option);

            option.AssertValidName();
            switch (option.Name.ToLower())
            {
                case "f":
                case "file":
                    Filename = option.AssertSingleParameter();
                    break;
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(Filename))
            {
                throw new ArgumentException("Filename (f or file) must be specified");
            }
        }

        public override string ToString()
        {
            return $"Filename: {Filename}";
        }
    }

}
