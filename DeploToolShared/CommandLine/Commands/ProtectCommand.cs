using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.CommandLine
{
    public class ProtectCommand : CliCommand
    {
        public ProtectCommand(string name) => this.Name = name;

        public string Data { get; private set; }

        public bool IsClear { get; set; }

        public override void Add(CliOption option)
        {
            base.Add(option);

            option.AssertValidName();
            switch (option.Name.ToLower())
            {
                case "encrypt":
                    Data = option.AssertSingleParameter();
                    IsClear = true;
                    break;

                case "decrypt":
                    Data = option.AssertSingleParameter();
                    IsClear = false;
                    break;
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(Data))
            {
                throw new ArgumentException("Data (encrypt or decrypt) must be specified");
            }
        }

        public override string ToString()
        {
            var direction = IsClear ? " is not " : " is ";
            return $"Data: {Data} {direction} encrypted";
        }
    }
}
