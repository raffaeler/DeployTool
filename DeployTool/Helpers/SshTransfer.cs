using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeployTool.Configuration;
using Renci.SshNet;

namespace DeployTool.Helpers
{
    public class SshTransfer
    {
        private ConnectionInfo _connectionInfo;

        public SshTransfer(SshConfiguration configuration)
        {
            AuthenticationMethod authenticationMethod;

            PrivateKeyFile[] privateKeys = null;
            if (configuration.PrivateKeys != null)
            {
                privateKeys = configuration.PrivateKeys
                    .Select(p => Load(p.PrivateKeyFile, p.PassPhrase))
                    .Where(p => p != null)
                    .ToArray();
            }

            if (privateKeys != null && privateKeys.Length > 0)
            {
                authenticationMethod =
                    new PrivateKeyAuthenticationMethod(configuration.Username, privateKeys);
            }
            else if (!string.IsNullOrEmpty(configuration.Password))
            {
                authenticationMethod =
                        new PasswordAuthenticationMethod(configuration.Username, configuration.Password);
            }
            else
            {
                authenticationMethod =
                    new NoneAuthenticationMethod(configuration.Username);
            }

            if (string.IsNullOrEmpty(configuration.ProxyHost))
            {
                _connectionInfo = new ConnectionInfo(
                    configuration.Host,
                    configuration.Port,
                    configuration.Username,
                    ProxyTypes.None,
                    null,
                    0,
                    null,
                    null,
                    authenticationMethod);
            }
            else
            {
                _connectionInfo = new ConnectionInfo(
                    configuration.Host,
                    configuration.Port,
                    configuration.Username,
                    ProxyTypes.Http,
                    configuration.ProxyHost,
                    configuration.ProxyPort,
                    configuration.ProxyUsername,
                    configuration.ProxyPassword,
                    authenticationMethod);
            }
        }

        public void SshCopyFileToRemote(FileInfo fileInfo, string remoteFolder)
        {
            SshCreateRemoteFolder(remoteFolder);
            var remoteFile = $"{remoteFolder.TrimEnd('/')}/{fileInfo.Name}";

            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                client.Downloading += Client_Downloading;

                try
                {
                    client.Upload(fileInfo, remoteFile);
                }
                finally
                {
                    client.Uploading -= Client_Uploading;
                    client.Downloading -= Client_Downloading;
                    client.Disconnect();
                }
            }
        }

        public void SshCreateRemoteFolder(string remoteFolder)
        {
            if (!remoteFolder.StartsWith("/") || !remoteFolder.StartsWith("~"))
            {
                throw new Exception($"RemoveRemote: The remote folder must be absolute. The request was instead: ({remoteFolder})");
            }

            var remoteFolders = remoteFolder.Split('/', StringSplitOptions.RemoveEmptyEntries);

            using (var client = new SftpClient(_connectionInfo))
            {
                try
                {
                    client.Connect();
                    string root = remoteFolder.StartsWith('~') ? "" : "/";
                    foreach (var folder in remoteFolders)
                    {
                        root += folder + "/";
                        client.CreateDirectory(root);
                    }
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }

        public void SshCopyDirectoryToRemote(DirectoryInfo localFolder, string remoteFolder, bool recurse, string remoteExecutable = null)
        {
            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                client.Downloading += Client_Downloading;

                try
                {
                    client.Upload(localFolder, remoteFolder);
                }
                finally
                {
                    client.Uploading -= Client_Uploading;
                    client.Downloading -= Client_Downloading;
                    client.Disconnect();
                }
            }

            if (!string.IsNullOrEmpty(remoteExecutable))
            {
                var remoteFullExecutable = $"{remoteFolder.TrimEnd('/')}/{remoteExecutable}";
                using (var client = new SftpClient(_connectionInfo))
                {
                    try
                    {
                        client.Connect();
                        client.ChangePermissions(remoteFullExecutable, 755);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }
        }

        public void SshRemoveRemoteFolderTree(string remoteFolder)
        {
            if (!remoteFolder.StartsWith("/") || !remoteFolder.StartsWith("~"))
            {
                throw new Exception($"RemoveRemote: The remote folder must be absolute. The request was instead: ({remoteFolder})");
            }

            if (remoteFolder == "/")
            {
                throw new Exception("RemoteRemote: Removing '/' can do huge damages... exiting");
            }

            if (remoteFolder == "~")
            {
                throw new Exception("RemoteRemote: Removing home ('~') can do huge damages... exiting");
            }

            using (var client = new SshClient(_connectionInfo))
            {
                try
                {
                    client.Connect();
                    var command = client.CreateCommand($"rm -rf {remoteFolder}");
                    command.Execute();
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }

        public string SshRunCommand(string remoteCommand)
        {
            using (var client = new SshClient(_connectionInfo))
            {
                try
                {
                    client.Connect();
                    var command = client.CreateCommand($"{remoteCommand}");
                    var output = command.Execute();
                    return output;
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }


        // TODO: Create remote directories
        //public void Transfer2(DirectoryInfo localFolder, bool recurse, string remoteFolder, string remoteFullExecutable)
        //{
        //    using (var client = new ScpClient(_connectionInfo))
        //    {
        //        client.Connect();
        //        client.Uploading += Client_Uploading;
        //        client.Downloading += Client_Downloading;

        //        var walker = new DirectoryWalker(localFolder, true, (f, r) =>
        //            {
        //                client.Upload(f, remoteFolder + "/" + r);
        //            });

        //        client.Disconnect();
        //    }

        //    if (!string.IsNullOrEmpty(remoteFullExecutable))
        //    {
        //        using (var client = new SftpClient(_connectionInfo))
        //        {
        //            client.ChangePermissions(remoteFullExecutable, 755);
        //        }
        //    }
        //}

        private void Client_Downloading(object sender, Renci.SshNet.Common.ScpDownloadEventArgs e)
        {
            Console.WriteLine($"{e.Filename} -> {e.Downloaded}");
        }

        private void Client_Uploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs e)
        {
            Console.WriteLine($"{e.Filename} -> {e.Uploaded}");
        }

        private PrivateKeyFile Load(string privateKeyFile, string passphrase)
        {
            try
            {
                var pvk = new PrivateKeyFile(privateKeyFile, passphrase);
                return pvk;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
                return null;
            }
        }
    }
}
