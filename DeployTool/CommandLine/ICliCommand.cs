using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool
{
    public interface ICliCommand
    {
        string Name { get; }

        IReadOnlyCollection<CliOption> Options { get; }

        void Add(CliOption option);

        void Validate();
    }
}
