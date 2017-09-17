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
        private ProjectHelper _project;
        private ExecuterManager _executerManager;

        public DeployApp()
        {
            //CliCommandFactory.Instance.Register(x => new WatchCommand(x), "watch");
            CliCommandFactory.Instance.Register(x => new ProtectCommand(x), "protect");
            CliCommandFactory.Instance.Register(x => new CreateCommand(x), "create");
            CliCommandFactory.Instance.Register(x => new RunCommand(x), "run");
            CliCommandFactory.Instance.Register(x => new InteractCommand(x), "interact");
            CliCommandFactory.Instance.Register(x => new HelpCommand(x), "help");
        }

        public int ProcessCLI(string[] args)
        {
            _executerManager = new ExecuterManager();

            var command = GetCommand(args);
            switch (command)
            {
                case HelpCommand helpCommand:
                    return ProcessHelpCommand(helpCommand);

                case ProtectCommand protectCommand:
                    return ProcessProtectCommand(protectCommand);
            }

            _project = EnsureProjectFolder();
            if (_project == null || command == null) return -1;

            _executerManager.Bag.SetValue(PipelineBag.ProjectName, _project.ProjectName);
            _executerManager.Bag.SetValue(PipelineBag.AssemblyName, _project.AssemblyName);

            switch (command)
            {
                case CreateCommand createCommand:
                    return ProcessCreateCommand(createCommand);

                case InteractCommand interactCommand:
                    return ProcessInteractCommand(interactCommand);

                case RunCommand runCommand:
                    return ProcessRunCommand(runCommand);

                default:
                    Console.WriteLine($"Unknown command {command.Name}");
                    return -1;
            }
        }

        private ProjectHelper EnsureProjectFolder()
        {
            ProjectHelper project = null;
            try
            {
                project = new ProjectHelper();
            }
            catch (Exception err)
            {
                ConsoleManager.WriteError(err.Message);
            }

            return project;
        }

        private ICliCommand GetCommand(string[] args)
        {
            ICliCommand command = null;
            try
            {
                command = CliCommand.FromArgs(CliCommandFactory.Instance, args);
                if (command == null)
                {
                    command = CliCommandFactory.Instance.Create("help");
                }
            }
            catch (Exception err)
            {
                ConsoleManager.WriteError(err.Message);
            }

            return command;
        }

        private DeployConfiguration ReadConfiguration(string filename)
        {
            try
            {
                var content = System.IO.File.ReadAllText(filename);
                var config = JsonHelper.Deserialize(content);
                return config;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            return null;
        }
    }
}
