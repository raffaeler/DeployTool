using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool
{
    public interface ICliCommandFactory
    {
        ICliCommand Create(string commandName);
    }
}
