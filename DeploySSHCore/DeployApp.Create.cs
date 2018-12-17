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
        private int ProcessCreateCommand(CreateCommand createCommand)
        {
            var extension = "." + Constants.DeployExtension;
            string filename = createCommand.Filename;
            if (!createCommand.Filename.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                filename = createCommand.Filename + extension;
            }

            DeployConfiguration deployConfiguration;
            if (createCommand.IsMinimal)
            {
                deployConfiguration = CreateMinimalisticSampleConfiguration();
            }
            else if (createCommand.IsEcho)
            {
                deployConfiguration = CreateEchoSampleConfiguration();
            }
            else
            {
                deployConfiguration = CreateSampleConfiguration();
            }

            var serialized = JsonHelper.Serialize(deployConfiguration, true);
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

        private DeployConfiguration CreateSampleConfiguration()
        {
            var deployConfiguration = new DeployConfiguration()
            {
                Description = "Test",

                Ssh = new SshConfiguration()
                {
                    Host = "machine-name",
                    Username = "username",
                    Password = "password",
                    EncryptedPassword = "...",
                    Port = 22,
                    ProxyHost = "proxy-host",
                    ProxyUsername = "proxy-username",
                    ProxyPassword = "proxy-password",
                    EncryptedProxyPassword = "...",
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

                    new SshCopyToRemoteAction()
                    {
                        DeleteRemoteFolder = false,
                        Recurse = true,
                        RemoteFolder = $"/{PipelineBag.ProjectName}",
                        LocalItems = new [] { PipelineBag.PublishDir, "sqlite.db" },
                    },

                    new SshRunCommandAction()
                    {
                        Command = "ls",
                    },

                    new SshRunAppAction()
                    {
                        Arguments = "hello",
                        RemoteFolder = $"/{PipelineBag.ProjectName}",
                        RemoteApp = $"{PipelineBag.AssemblyName}",
                    },

                    new SshSyncRemoteAction()
                    {
                        LocalFolder = "somefolder",
                        RemoteFolder = $"someremotefolder",
                        Recurse = false,
                        DeleteRemoteOrphans = false,
                    },
                },
            };

            return deployConfiguration;
        }

        private DeployConfiguration CreateMinimalisticSampleConfiguration()
        {
            var deployConfiguration = new DeployConfiguration()
            {
                Description = "Test",

                Ssh = new SshConfiguration()
                {
                    Host = "machine-name",
                    Username = "username",
                    Password = "password",
                    EncryptedPassword = "...",
                },

                Actions = new List<IAction>()
                {
                    new DotnetPublishAction()
                    {
                        Configuration = "Release",
                        IsSelfContained = true,
                        RuntimeIdentifier = "linux-arm",
                    },

                    new SshCopyToRemoteAction()
                    {
                        DeleteRemoteFolder = false,
                        Recurse = true,
                        RemoteFolder = $"{PipelineBag.ProjectName}",
                        LocalItems = new [] { PipelineBag.PublishDir },
                    },
                },
            };

            return deployConfiguration;
        }

        private DeployConfiguration CreateEchoSampleConfiguration()
        {
            var deployConfiguration = new DeployConfiguration()
            {
                Description = "Test",

                Ssh = new SshConfiguration()
                {
                    Host = "machine-name",
                    Username = "username",
                    Password = "password",
                    EncryptedPassword = "...",
                },

                Actions = new List<IAction>()
                {
                    new SshSyncRemoteAction()
                    {
                        LocalFolder = "somefolder",
                        RemoteFolder = $"someremotefolder",
                        Recurse = false,
                        DeleteRemoteOrphans = false,
                    },
                },
            };

            return deployConfiguration;
        }
    }
}
