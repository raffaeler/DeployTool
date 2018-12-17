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
        private ProjectHelper _project;
        private ExecuterManager _executerManager;

        public DeployApp()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            //CliCommandFactory.Instance.Register(x => new WatchCommand(x), "watch");
            //CliCommandFactory.Instance.Register(x => new ProtectCommand(x), "protect");   // obsoleted
            CliCommandFactory.Instance.Register(x => new EncryptCommand(x), "encrypt");
            CliCommandFactory.Instance.Register(x => new DecryptCommand(x), "decrypt");
            CliCommandFactory.Instance.Register(x => new CreateCommand(x), "create");
            CliCommandFactory.Instance.Register(x => new RunCommand(x), "run");
            CliCommandFactory.Instance.Register(x => new InteractCommand(x), "interact");
            CliCommandFactory.Instance.Register(x => new PreviewCommand(x), "preview");
            CliCommandFactory.Instance.Register(x => new HelpCommand(x), "help");
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ConsoleManager.RestoreColors();
        }

        public int ProcessCLI(string[] args)
        {
            _executerManager = new ExecuterManager();
            _project = EnsureProjectFolder();

            if (_project != null)
            {
                _executerManager.Bag.SetValue(PipelineBag.ProjectName, _project.ProjectName);
                _executerManager.Bag.SetValue(PipelineBag.AssemblyName, _project.AssemblyName);
                _executerManager.Bag.SetValue(PipelineBag.ProjectDir, _project.ProjectDir);
            }

            var command = GetCommand(args);
            switch (command)
            {
                case HelpCommand helpCommand:
                    return ProcessHelpCommand(helpCommand);

                // obsoleted
                //case ProtectCommand protectCommand:
                //    return ProcessProtectCommand(protectCommand);

                case CreateCommand createCommand:
                    return ProcessCreateCommand(createCommand);

                case EncryptCommand encryptCommand:
                    return ProcessEncryptCommand(encryptCommand);

                case DecryptCommand decryptCommand:
                    return ProcessDecryptCommand(decryptCommand);
            }

            if (command == null) return -1;

            switch (command)
            {
                case InteractCommand interactCommand:
                    return ProcessInteractCommand(interactCommand);

                case PreviewCommand previewCommand:
                    return ProcessPreviewCommand(previewCommand);

                case RunCommand runCommand:
                    return ProcessRunCommand(runCommand, false);

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
