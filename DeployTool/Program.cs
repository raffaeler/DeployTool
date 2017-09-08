using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;

namespace DeployTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //CliCommandFactory.Instance.Register(x => new WatchCommand(x), "watch");
            CliCommandFactory.Instance.Register(x => new CreateCommand(x), "create");
            CliCommandFactory.Instance.Register(x => new RunCommand(x), "run");
            CliCommandFactory.Instance.Register(x => new InteractCommand(x), "interact");
            CliCommandFactory.Instance.Register(x => new HelpCommand(x), "help");

            ProcessCLI(args);
            return;


            BuildConfig();
            //DumpProject();


            var config = new SshConfiguration();
            config.Host = "colibri-t30";
            config.Username = "root";
            config.Password = null;
            config.Port = 22;
            config.ProxyHost = null;
            config.ProxyUsername = null;
            config.ProxyPassword = null;
            config.ProxyPort = 8080;
            config.PrivateKeys = null;
            var transfer = new SshTransfer(config);

            transfer.CopyFileToRemote(new System.IO.FileInfo(@"h:\3d\raf.txt"), "/temp/1/2/3/aaaa/caaaaa");
            transfer.ExecuteRemoteCommand("ls");
            transfer.RemoveRemoteFolderTree("/temp");
            //transfer.Transfer(new System.IO.DirectoryInfo(@"H:\3D"), "/temp", true, "/temp/raf.txt");

            //DirectoryWalker walker = new DirectoryWalker(new System.IO.DirectoryInfo("h:\\temp"), true, (f, r) =>
            //{
            //    Console.WriteLine($"{f.FullName}\t{r}");
            //});

            Console.WriteLine("Hello World!");
        }

        private static void BuildConfig()
        {

            var manager = new ExecuterManager();
            //manager.Execute(deployConfiguration);
        }

        private static void DumpProject()
        {
            var command = new DotnetPublishAction();
            command.IsSelfContained = true;
            command.RuntimeIdentifier = "linux-arm";
            var exec = new DotNetPublishExecuter(command);
            exec.Execute(null);

            //var executer = new DotNetExecuter("publish", "-c Release -r linux-arm --self-contained=false");
            //executer.ExecuteAndWait();
        }

        private static int ProcessCLI(string[] args)
        {
            var project = EnsureProjectFolder();
            var command = GetCommand(args);
            if (project == null || command == null) return -1;

            switch (command)
            {
                case HelpCommand helpCommand:
                    return ProcessHelpCommand(helpCommand);

                case CreateCommand generateCommand:
                    return ProcessGenerateCommand(generateCommand);

                case InteractCommand interactCommand:
                    return ProcessInteractCommand(interactCommand);

                case RunCommand runCommand:
                    return ProcessRunCommand(runCommand);

                default:
                    Console.WriteLine($"Unknown command {command.Name}");
                    return -1;
            }
        }

        private static int ProcessHelpCommand(HelpCommand helpCommand)
        {
            Console.WriteLine("dotnet deploy tool v1.0 by @raffaeler (https://github.com/raffaeler/DeployTool)");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy create -f <filename>.deploy");
            Console.WriteLine("    Create a new configuration file with sample data");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy run -f <filename>.deploy");
            Console.WriteLine("    Process the actions described in the configuration file");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy interact");
            Console.WriteLine("    Show an interactive menu allowing to process/run the desired configuration");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy help");
            Console.WriteLine("    Show this help");
            Console.WriteLine("");
            return 0;
        }

        private static int ProcessGenerateCommand(CreateCommand generateCommand)
        {
            var extension = "." + Constants.DeployExtension;
            string filename = generateCommand.Filename;
            if (!generateCommand.Filename.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                filename = generateCommand.Filename + extension;
            }

            var deployConfiguration = CreateSampleConfiguration();
            var serialized = JsonHelper.Serialize(deployConfiguration);
            try
            {
                System.IO.File.WriteAllText(filename, serialized);
            }
            catch (Exception err)
            {
                ConsoleManager.WriteError($"Error writing the configuration file: {err.Message}");
            }

            return 0;
        }

        private static int ProcessInteractCommand(InteractCommand interactCommand)
        {
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            var files = di
                .GetFiles($"*.{Constants.DeployExtension}")
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f.Name))
                .OrderBy(n => n)
                .ToArray();

            string current;
            while ((current = ConsoleManager.RunLoop("Select a configuration file:", files)) != null)
            {
                var config = ReadConfiguration(current);
                ProcessConfiguration(config);
            }

            return 0;
        }

        private static int ProcessRunCommand(RunCommand runCommand)
        {
            var config = ReadConfiguration(runCommand.Filename);
            return ProcessConfiguration(config);
        }

        private static int ProcessConfiguration(DeployConfiguration deployConfiguration)
        {

            return 0;
        }

        private static DeployConfiguration ReadConfiguration(string filename)
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

        private static ProjectHelper EnsureProjectFolder()
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

        private static ICliCommand GetCommand(string[] args)
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

        private static DeployConfiguration CreateSampleConfiguration()
        {
            var deployConfiguration = new DeployConfiguration()
            {
                Description = "Test",

                Ssh = new SshConfiguration()
                {
                    Host = "machine-name",
                    Username = "username",
                    Password = "password",
                    Port = 22,
                    ProxyHost = "proxy-host",
                    ProxyUsername = "proxy-username",
                    ProxyPassword = "proxy-password",
                    ProxyPort = 8080,
                    PrivateKeys = new PrivateKeyData[]
                    {
                        new PrivateKeyData()
                        {
                            PrivateKeyFile = "privatekeyfile.key",
                            PassPhrase = "passphrase",
                        },
                    }
                },

                Actions = new List<IAction>()
                {
                    new DotnetPublishAction()
                    {
                        Configuration = "Release",
                        IsSelfContained = true,
                        RuntimeIdentifier = "linux-arm",

                        OutputFolder = "publish-linux-arm",
                        IsForce = false,
                        IsNoDependencies = false,
                        IsNoRestore = false,
                        Manifest = "manifest.xml",
                        TargetFramework = "netcoreapp2.0",
                        Verbosity = "minimal",
                        VersionSuffix = "suffix",
                    },

                    new CopyToRemoteAction()
                    {
                        DeleteRemoteFolder = true,
                        Recurse = true,
                        RemoteFolder = "/temp",
                        LocalItems = new [] { null, "sqlite.db" },
                    },

                    new ExecuteCommandAction()
                    {
                        Command = "ls",
                    },

                    new ExecuteRemoteAppAction()
                    {
                        Arguments = "hello",
                        RemoteFolder = null,
                        RemoteApp = null,
                    },
                },
            };

            return deployConfiguration;
        }

    }

}
