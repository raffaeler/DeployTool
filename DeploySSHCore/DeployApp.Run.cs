using System;
using System.Collections.Generic;
using System.Text;

using DeploySSH.CommandLine;
using DeploySSH.Configuration;
using DeploySSH.Executers;
using DeploySSH.Helpers;

namespace DeploySSH
{
    public partial class DeployApp
    {
        private int ProcessRunCommand(RunCommand runCommand, bool preview)
        {
            var config = ReadConfiguration(runCommand.Filename);
            return ProcessConfiguration(config, preview);
        }

        private int ProcessConfiguration(DeployConfiguration deployConfiguration, bool preview)
        {
            return _executerManager.Execute(deployConfiguration, preview);
        }
    }
}
