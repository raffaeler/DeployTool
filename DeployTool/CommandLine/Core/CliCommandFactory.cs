using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool
{
    public class CliCommandFactory : ICliCommandFactory
    {
        public static readonly CliCommandFactory Instance = new CliCommandFactory();
        public IDictionary<string, Func<string, ICliCommand>> Factories { get; private set; }
        private CliCommandFactory()
        {
            Factories = new Dictionary<string, Func<String, ICliCommand>>();
        }

        public void Register(Func<string, ICliCommand> factory, params string[] names)
        {
            foreach (var name in names)
            {
                Factories[name] = factory;
            }
        }

        public ICliCommand Create(string commandName)
        {
            if (!Factories.TryGetValue(commandName, out Func<string, ICliCommand> factory))
                return null;
            return factory(commandName);
        }
    }
}
