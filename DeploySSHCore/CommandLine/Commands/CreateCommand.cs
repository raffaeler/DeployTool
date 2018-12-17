using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.CommandLine
{
    public class CreateCommand : CliCommand
    {
        public CreateCommand(string name) => this.Name = name;

        public string Filename { get; private set; }
        public bool IsMinimal { get; private set; }
        public bool IsEcho { get; private set; }

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

                case "m":
                case "minimal":
                    IsMinimal = true;
                    break;

                case "e":
                case "echo":
                    IsEcho = true;
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
