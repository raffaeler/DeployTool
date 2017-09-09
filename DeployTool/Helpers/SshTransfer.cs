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
        private string _lastError;
        private long _totalsize;
        private long _relativesize;
        private long _lastFileSize;
        private long _lastFilePartial;
        private string _lastFile;
        private int _cursorTop;

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

        public (bool isError, string output) SshCopyFileToRemote(FileInfo fileInfo, string remoteFolder)
        {
            SshCreateRemoteFolder(remoteFolder);
            var remoteFile = $"{remoteFolder.TrimEnd('/')}/{fileInfo.Name}";

            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                //client.Downloading += Client_Downloading;
                client.ErrorOccurred += Client_ErrorOccurred;

                var state = ConsoleManager.GetConsoleState();
                _cursorTop = state.Top;
                ConsoleManager.ClearLine(state.Top);

                try
                {
                    client.Upload(fileInfo, remoteFile);
                    if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                }
                finally
                {
                    client.ErrorOccurred -= Client_ErrorOccurred;
                    client.Uploading -= Client_Uploading;
                    //client.Downloading -= Client_Downloading;
                    //client.Disconnect();
                }
            }

            return (false, "Upload ok");
        }

        public void SshCreateRemoteFolder(string remoteFolder)
        {
            if (!remoteFolder.StartsWith("/") && !remoteFolder.StartsWith("~"))
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
                    //client.Disconnect();
                }
            }
        }

        public (bool isError, string output) SshCopyDirectoryToRemote(DirectoryInfo localFolder, string remoteFolder, bool recurse, string remoteExecutable = null)
        {
            _lastError = null;
            ResetProgress(localFolder.GetSize());
            _relativesize = 0;

            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                //client.Downloading += Client_Downloading;
                client.ErrorOccurred += Client_ErrorOccurred;

                var state = ConsoleManager.GetConsoleState();
                _cursorTop = state.Top;
                ConsoleManager.ClearLine(state.Top);

                try
                {
                    client.Upload(localFolder, remoteFolder);
                    if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                }
                finally
                {
                    state.Top = Console.CursorTop + 1;
                    ConsoleManager.SetConsoleState(state);
                    client.ErrorOccurred -= Client_ErrorOccurred;
                    client.Uploading -= Client_Uploading;
                    //client.Downloading -= Client_Downloading;
                    //client.Disconnect();
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
                        client.ErrorOccurred += Client_ErrorOccurred;
                        client.ChangePermissions(remoteFullExecutable, 755);
                        if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                    }
                    finally
                    {
                        client.ErrorOccurred -= Client_ErrorOccurred;
                        //client.Disconnect();
                    }
                }
            }

            return (false, "Upload + Permission ok");
        }


        public (bool isError, string output) SshRemoveRemoteFolderTree(string remoteFolder)
        {
            if (!remoteFolder.StartsWith("/") && !remoteFolder.StartsWith("~"))
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
                    var output1 = command.Execute();
                    var output = command.Result;
                    if (!string.IsNullOrEmpty(command.Error))
                    {
                        return (true, command.Error);
                    }

                    return (false, output);
                }
                finally
                {
                    //client.Disconnect();
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
                    //client.Disconnect();
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
            UpdateProgress(e.Filename, e.Size, e.Downloaded);
        }

        private void Client_Uploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs e)
        {
            UpdateProgress(e.Filename, e.Size, e.Uploaded);
        }

        private void ResetProgress(long totalSize)
        {
            _lastFile = null;
            _lastFileSize = 0;
            _lastFilePartial = 0;
            _relativesize = 0;
            _totalsize = totalSize;
        }

        private void UpdateProgress(string filename, long size, long partial)
        {
            if (_lastFile != filename)
            {
                _lastFilePartial = 0;

                //_relativesize += _lastFileSize;
                //_lastFileSize = size;
                _lastFile = filename;
            }

            var delta = partial - _lastFilePartial;
            _lastFilePartial = partial;
            _relativesize += delta;

            var percent = _relativesize * 100 / _totalsize;
            var msg = $"{percent}% {filename} ";
            var filler = new string(' ', Console.WindowWidth - msg.Length-1);
            ConsoleManager.WriteAt(0, _cursorTop, msg + filler);
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            var _lastError = e.Exception.Message;
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
