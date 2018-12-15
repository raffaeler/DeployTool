using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeployTool.Configuration;
using Renci.SshNet;
using DeploToolShared.Helpers;

namespace DeployTool.Helpers
{
    public class SshTransfer
    {
        private ConnectionInfo _connectionInfo;
        private string _lastError;
        private SshProgress _progress;
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
            else
            {
                var password = GetPassword(configuration);
                if (!string.IsNullOrEmpty(password))
                {
                    authenticationMethod =
                            new PasswordAuthenticationMethod(configuration.Username, password);
                }
                else
                {
                    authenticationMethod =
                        new NoneAuthenticationMethod(configuration.Username);
                }
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
                var proxyPassword = GetProxyPassword(configuration);

                _connectionInfo = new ConnectionInfo(
                    configuration.Host,
                    configuration.Port,
                    configuration.Username,
                    ProxyTypes.Http,
                    configuration.ProxyHost,
                    configuration.ProxyPort,
                    configuration.ProxyUsername,
                    proxyPassword,
                    authenticationMethod);
            }
        }

        private string GetPassword(SshConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.EncryptedPassword))
            {
                try
                {
                    var password = DPApiHelper.Decrypt(configuration.EncryptedPassword);
                    return password;
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Encrypted password is not valid, fallback to password: {err.Message}");
                }
            }

            return configuration.Password;
        }

        private string GetProxyPassword(SshConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.EncryptedProxyPassword))
            {
                try
                {
                    var proxyPassword = DPApiHelper.Decrypt(configuration.EncryptedProxyPassword);
                    return proxyPassword;
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Encrypted proxy password is not valid, fallback to proxy password: {err.Message}");
                }
            }

            return configuration.ProxyPassword;
        }

        public (bool isError, string output) SshCopyFileToRemote(FileInfo fileInfo, string remoteFolder)
        {
            _progress = new SshProgress(fileInfo.Length, 1, OnTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);
            var rf = $"{remoteFolder.TrimEnd('/')}/";

            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                client.ErrorOccurred += Client_ErrorOccurred;
                SshCreateRemoteFolder(client, remoteFolder);

                var state = ConsoleManager.GetConsoleState();
                _cursorTop = state.Top;
                ConsoleManager.ClearLine(state.Top);

                try
                {
                    using (var fs = File.OpenRead(fileInfo.FullName))
                    {
                        client.UploadFile(fs, rf + fileInfo.Name, true, length =>
                            _progress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length));
                    }

                    if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                    _progress.UpdateProgressFinal();
                    _cursorTop++;
                    ConsoleManager.SetConsoleState(0, _cursorTop);
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Error on remote Upload: {err.ToString()}");
                    throw;
                }
                finally
                {
                    client.ErrorOccurred -= Client_ErrorOccurred;
                }
            }

            return (false, "Upload ok");
        }

        public bool SshCreateRemoteFolder(SftpClient client, string remoteFolder)
        {
            bool wasCreated = false;
            if (!remoteFolder.StartsWith("/") && !remoteFolder.StartsWith("~"))
            {
                throw new Exception($"RemoveRemote: The remote folder must be absolute. The request was instead: ({remoteFolder})");
            }

            var remoteFolders = remoteFolder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                string root = remoteFolder.StartsWith("~") ? "" : "/";
                foreach (var folder in remoteFolders)
                {
                    root += folder + "/";
                    //if (!client.Exists(root))
                    //{
                    try
                    {
                        client.CreateDirectory(root);
                        wasCreated = true;
                    }
                    catch (Exception)
                    {
                        wasCreated = false;
                    }
                    //}
                }
            }
            catch (Exception err)
            {
                throw new Exception($"Cannot create the remote directory: {err.Message}");
            }
            finally
            {
            }

            return wasCreated;
        }

        public bool SshCreateRemoteFolder(string remoteFolder)
        {
            bool wasCreated = false;
            if (!remoteFolder.StartsWith("/") && !remoteFolder.StartsWith("~"))
            {
                throw new Exception($"RemoveRemote: The remote folder must be absolute. The request was instead: ({remoteFolder})");
            }

            var remoteFolders = remoteFolder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                try
                {
                    string root = remoteFolder.StartsWith("~") ? "" : "/";
                    foreach (var folder in remoteFolders)
                    {
                        root += folder + "/";
                        if (!client.Exists(root))
                        {
                            client.CreateDirectory(root);
                            wasCreated = true;
                        }
                    }
                }
                catch (Exception err)
                {
                    throw new Exception($"Cannot create the remote directory: {err.Message}");
                }
                finally
                {
                }

                return wasCreated;
            }
        }

        public (bool isError, string output) SshCopyDirectoryToRemote(DirectoryInfo localFolder, string remoteFolder, bool recurse, string remoteExecutable = null)
        {
            _lastError = null;
            var folderInfo = localFolder.GetSizeAndAmount();
            _progress = new SshProgress(folderInfo.size, folderInfo.amount, OnTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                client.ErrorOccurred += Client_ErrorOccurred;

                var state = ConsoleManager.GetConsoleState();
                _cursorTop = state.Top;
                ConsoleManager.ClearLine(state.Top);

                try
                {
                    client.Upload(localFolder, remoteFolder);
                    if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                    _progress.UpdateProgressFinal();
                    //_cursorTop++;
                    //ConsoleManager.SetConsoleState(0, _cursorTop);
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Error on Upload: {err.ToString()}");
                }
                finally
                {
                    state.Top = Console.CursorTop + 1;
                    ConsoleManager.SetConsoleState(state);
                    client.ErrorOccurred -= Client_ErrorOccurred;
                    client.Uploading -= Client_Uploading;
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
                        if (client.Exists(remoteFullExecutable))
                        {
                            client.ChangePermissions(remoteFullExecutable, 755);
                            if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                        }
                    }
                    finally
                    {
                        client.ErrorOccurred -= Client_ErrorOccurred;
                    }
                }
            }

            return (false, "Upload + Permission ok");
        }

        public (bool isError, string output) SshCopyDirectoryToRemote2(DirectoryInfo localFolder, string remoteFolder, bool recurse, string remoteExecutable = null)
        {
            _lastError = null;
            var folderInfo = localFolder.GetSizeAndAmount();
            _progress = new SshProgress(folderInfo.size, folderInfo.amount, OnTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            var rf = $"{remoteFolder.TrimEnd('/')}/";
            int count = 0;

            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                client.ErrorOccurred += Client_ErrorOccurred;
                var walker = new DirectoryWalker(localFolder, recurse);
                try
                {
                    // swallowing the "already exists" error
                    // not using the "Exists" method because it throws an (internal) exception too
                    client.CreateDirectory(remoteFolder);
                }
                catch (Exception) { }

                var state = ConsoleManager.GetConsoleState();
                _cursorTop = state.Top;
                ConsoleManager.ClearLine(state.Top);

                try
                {
                    var isSuccess = walker.Walk((fileInfo, relative) =>
                    {
                        try
                        {
                            using (var fs = File.OpenRead(fileInfo.FullName))
                            {
                                client.UploadFile(fs, rf + relative, true, length =>
                                    _progress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length));
                            }

                            ++count;
                            return true;
                        }
                        catch (Exception err)
                        {
                            _lastError = err.Message;
                            return false;
                        }
                    },
                    (directoryInfo, relative) =>
                    {
                        //SshCreateRemoteFolder(rf + relative);
                        client.CreateDirectory(rf + relative);
                        return true;
                    });

                    if (!isSuccess || !string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                    _progress.UpdateProgressFinal(count);
                    _cursorTop++;
                    ConsoleManager.SetConsoleState(0, _cursorTop);
                }
                catch (Exception err)
                {
                    _lastError = $"Error copying {_progress.CurrentFilename}: {err.Message}";
                    return (true, _lastError);
                }
                finally
                {
                    state.Top = Console.CursorTop + 1;
                    ConsoleManager.SetConsoleState(state);
                    client.ErrorOccurred -= Client_ErrorOccurred;
                }
            }


            /*
            // This is the previous implementation. It cannot be used because of a bug in the SSH library
            // which double the remote folder name

            */
            if (!string.IsNullOrEmpty(remoteExecutable))
            {
                var remoteFullExecutable = $"{remoteFolder.TrimEnd('/')}/{remoteExecutable}";
                using (var client = new SftpClient(_connectionInfo))
                {
                    try
                    {
                        client.Connect();
                        client.ErrorOccurred += Client_ErrorOccurred;
                        if (client.Exists(remoteFullExecutable))
                        {
                            client.ChangePermissions(remoteFullExecutable, 755);
                            if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                        }
                    }
                    catch (Exception err)
                    {
                        _lastError = $"Error assigning the permissions to {remoteFullExecutable}: {err.Message}";
                        return (true, _lastError);
                    }
                    finally
                    {
                        client.ErrorOccurred -= Client_ErrorOccurred;
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
                    //Console.WriteLine("Deleting remote folder");
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
                catch (Exception err)
                {
                    Console.WriteLine($"Error deleting remote folder: {err.ToString()}");
                    throw;
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
                catch (Exception err)
                {
                    Console.WriteLine($"Error running command {remoteCommand}: {err.ToString()}");
                    throw;
                }
                finally
                {
                }
            }
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            _lastError = e.Exception.Message;
        }

        private void Client_Downloading(object sender, Renci.SshNet.Common.ScpDownloadEventArgs e)
        {
            _progress.UpdateProgress(e.Filename, e.Size, e.Downloaded);
        }

        private void Client_Uploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs e)
        {
            _progress.UpdateProgress(e.Filename, e.Size, e.Uploaded);
        }

        private void OnTransfer(SshProgress progress)
        {
            ConsoleManager.WriteAt(0, _cursorTop, progress.FormattedString);
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
                Console.WriteLine($"Error loading PrivateKeyFile: {err.ToString()}");
                return null;
            }
        }
    }
}
