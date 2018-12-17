using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH
{
    public class WatchCommand : CliCommand
    {
        public WatchCommand(string name) => this.Name = name;

        public string RuntimeIdentifier { get; private set; }
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public override void Add(CliOption option)
        {
            base.Add(option);

            option.AssertValidName();
            switch (option.Name.ToLower())
            {
                case "h":
                case "host":
                    Host = option.AssertSingleParameter();
                    break;

                case "r":
                case "rid":
                    RuntimeIdentifier = option.AssertSingleParameter();
                    break;

                case "u":
                case "user":
                    Username = option.AssertSingleParameter();
                    break;

                case "p":
                case "password":
                    Password = option.AssertSingleParameter();
                    break;
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(Host) ||
                string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(RuntimeIdentifier))
            {
                throw new ArgumentException("Host (h), Username (u) and RuntimeIdentifier (r) must be specified");
            }
        }

        public override string ToString()
        {
            return $"Host: {Host}, Username: {Username}, Password:{Password}, RID: {RuntimeIdentifier}";
        }
    }
}
