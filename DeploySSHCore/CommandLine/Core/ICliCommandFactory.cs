using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH
{
    public interface ICliCommandFactory
    {
        ICliCommand Create(string commandName);
    }
}
