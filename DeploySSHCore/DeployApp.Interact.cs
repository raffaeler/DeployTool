using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeploySSH.CommandLine;
using DeploySSH.Configuration;
using DeploySSH.Executers;
using DeploySSH.Helpers;

namespace DeploySSH
{
    public partial class DeployApp
    {
        private int ProcessInteractCommand(InteractCommand interactCommand)
        {
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            var files = di
                .GetFiles($"*.{Constants.DeployExtension}")
                .OrderBy(n => n.Name)
                .ToArray();

            System.IO.FileInfo current;
            var heading = _project == null ? $"No project found" : $"{_project.ProjectName}";
            while ((current = ConsoleManager.RunLoop($"{heading}\r\nSelect a configuration file or 'q' to quit", files)) != null)
            {
                var config = ReadConfiguration(current.FullName);
                ProcessConfiguration(config, false);
            }

            return 0;
        }



    }
}
