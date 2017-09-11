using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool
{
    public class CliCommand : ICliCommand
    {
        private List<CliOption> _options;
        public CliCommand()
        {
            _options = new List<CliOption>();
        }

        public static ICliCommand FromArgs(ICliCommandFactory factory, string[] args)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ICliCommand command = null;
            CliOption currentOption = null;
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("-") || arg.StartsWith("--"))
                {
                    var stripped = arg.TrimStart('-');
                    if (command == null)
                    {
                        command = factory.Create(stripped);
                        continue;
                    }

                    if (currentOption != null)
                    {
                        command.Add(currentOption);
                    }

                    currentOption = new CliOption();
                    currentOption.Name = stripped;
                }
                else
                {
                    if (command == null)
                    {
                        command = factory.Create(arg);
                        continue;
                    }

                    if (currentOption == null)
                    {
                        throw new ArgumentException($"The argument {arg} was unexpected");
                    }

                    currentOption.RawParameters.Add(arg);
                }
            }

            if (currentOption != null)
            {
                command.Add(currentOption);
            }

            command?.Validate();
            return command;
        }

        public string Name { get; set; }
        public IReadOnlyCollection<CliOption> Options { get => _options; }
        public virtual void Add(CliOption option)
        {
            _options.Add(option);
        }

        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("An unnamed command was found");
            }
        }

        public override string ToString()
        {
            return $"CliCommand {Name}:{string.Join(",", Options)}";
        }
    }
}
