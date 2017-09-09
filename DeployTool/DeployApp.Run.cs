using System;
using System.Collections.Generic;
using System.Text;

using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;

namespace DeployTool
{
    internal partial class DeployApp
    {
        private int ProcessRunCommand(RunCommand runCommand)
        {
            var config = ReadConfiguration(runCommand.Filename);
            return ProcessConfiguration(config);
        }

        private int ProcessConfiguration(DeployConfiguration deployConfiguration)
        {

            return 0;
        }

    }
}
