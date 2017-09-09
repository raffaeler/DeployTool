using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;

namespace DeployTool
{
    internal partial class DeployApp
    {
        private int ProcessInteractCommand(InteractCommand interactCommand)
        {
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            var files = di
                .GetFiles($"*.{Constants.DeployExtension}")
                .OrderBy(n => n.Name)
                .ToArray();

            System.IO.FileInfo current;
            while ((current = ConsoleManager.RunLoop("Select a configuration file or 'q' to quit", files)) != null)
            {
                var config = ReadConfiguration(current.FullName);
                ProcessConfiguration(config);
            }

            return 0;
        }



    }
}
