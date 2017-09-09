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
                .Select(f => GetTitle(f))
                .OrderBy(n => n)
                .ToArray();

            string current;
            while ((current = ConsoleManager.RunLoop("Select a configuration file or 'q' to quit", files)) != null)
            {
                var config = ReadConfiguration(current);
                ProcessConfiguration(config);
            }

            return 0;
        }

        private string GetTitle(System.IO.FileInfo fileInfo)
        {
            var simpleName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
            var content = System.IO.File.ReadAllText(fileInfo.FullName);
            var config = JsonHelper.Deserialize(content);
            if (config == null || config.Description == null)
            {
                return simpleName;
            }

            var actions = string.Join(", ", config.Actions.Select(a => a.GetShortActionName()));
            if (string.IsNullOrEmpty(actions))
            {
                return $"{simpleName} ({config.Description}: no actions)";
            }

            return $"{simpleName} ({config.Description}: {actions})";
        }


    }
}
