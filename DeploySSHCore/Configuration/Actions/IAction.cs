using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Configuration
{
    public interface IAction
    {
        string ActionName { get; }

        string GetShortActionName();
    }
}
