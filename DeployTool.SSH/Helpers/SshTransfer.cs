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
        private static char[] _eol = new char[] { '\n', '\r' };
        private ConnectionInfo _connectionInfo;
        private string _lastError;
        private SshProgress _progress;
        //private int _cursorTop;

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

        public void ConnectScp(Action<ScpClient> clientAction)
        {
            if (clientAction == null) throw new ArgumentNullException(nameof(clientAction));
            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                clientAction(client);
            }
        }

        public void ConnectSftp(Action<SftpClient> clientAction)
        {
            if (clientAction == null) throw new ArgumentNullException(nameof(clientAction));
            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                clientAction(client);
            }
        }

        public (bool isError, string output) SshCopyFileToRemote(
            FileInfo fileInfo,
            string remoteFolder,
            Action<SshProgress> onTransfer)
        {
            _progress = new SshProgress("SshCopyFileToRemote", fileInfo.Length, 1, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);
            var rf = $"{remoteFolder.TrimEnd('/')}/";

            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                client.ErrorOccurred += Client_ErrorOccurred;
                SshCreateRemoteFolder(client, remoteFolder);

                _progress.UpdateConnected(1);
                //var state = ConsoleManager.GetConsoleState();
                //_cursorTop = state.Top;
                //ConsoleManager.ClearLine(state.Top);

                try
                {
                    using (var fs = File.OpenRead(fileInfo.FullName))
                    {
                        client.UploadFile(fs, rf + fileInfo.Name, true, length =>
                            _progress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length));
                    }

                    if (!string.IsNullOrEmpty(_lastError)) return (true, _lastError);
                    _progress.UpdateProgressFinal();
                    //_cursorTop++;
                    //ConsoleManager.SetConsoleState(0, _cursorTop);
                }
                catch (Exception err)
                {
                    //Console.WriteLine($"Error on remote Upload: {err.ToString()}");
                    _progress.UpdateProgressErrorAborting(err);
                    throw;
                }
                finally
                {
                    client.ErrorOccurred -= Client_ErrorOccurred;
                }
            }

            _progress.UpdateDisconnected(0);
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
                    string root = string.Empty;
                    var isFirst = true;

                    foreach (var folder in remoteFolders)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                            if (folder == "~") continue;

                            root = "/";
                        }

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

        public (bool isError, string output) SshCopyDirectoryToRemote(
            DirectoryInfo localFolder, string remoteFolder,
            bool recurse, Action<SshProgress> onTransfer = null, string remoteExecutable = null)
        {
            _lastError = null;
            var folderInfo = localFolder.GetSizeAndAmount();
            _progress = new SshProgress("SshCopyDirectoryToRemote", folderInfo.size, folderInfo.amount, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            using (var client = new ScpClient(_connectionInfo))
            {
                client.Connect();
                client.Uploading += Client_Uploading;
                client.ErrorOccurred += Client_ErrorOccurred;

                //var state = ConsoleManager.GetConsoleState();
                //_cursorTop = state.Top;
                //ConsoleManager.ClearLine(state.Top);
                _progress.UpdateConnected(1);

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
                    //state.Top = Console.CursorTop + 1;
                    //ConsoleManager.SetConsoleState(state);
                    client.ErrorOccurred -= Client_ErrorOccurred;
                    client.Uploading -= Client_Uploading;
                }
            }

            _progress.UpdateDisconnected(0);

            if (!string.IsNullOrEmpty(remoteExecutable))
            {
                var remoteFullExecutable = $"{remoteFolder.TrimEnd('/')}/{remoteExecutable}";
                using (var client = new SftpClient(_connectionInfo))
                {
                    try
                    {
                        client.Connect();
                        _progress.UpdateConnected(1);

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

                _progress.UpdateDisconnected(0);
            }

            return (false, "Upload + Permission ok");
        }

        public (bool isError, string output) SshCopyDirectoryToRemote2(
            DirectoryInfo localFolder, string remoteFolder,
            bool recurse, Action<SshProgress> onTransfer = null, string remoteExecutable = null)
        {
            _lastError = null;
            var folderInfo = localFolder.GetSizeAndAmount();
            _progress = new SshProgress("SshCopyDirectoryToRemote2", folderInfo.size, folderInfo.amount, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            var rf = $"{remoteFolder.TrimEnd('/')}/";
            int count = 0;

            using (var clicmd = new SshClient(_connectionInfo))
            {
                using (var client = new SftpClient(_connectionInfo))
                {
                    client.Connect();
                    client.ErrorOccurred += Client_ErrorOccurred;
                    _progress.UpdateConnected(1);

                    var walker = new DirectoryWalker(localFolder, recurse);
                    if (!RemoteFolderExists(remoteFolder, clicmd))
                        client.CreateDirectory(remoteFolder);
                    //try
                    //{
                    //    // swallowing the "already exists" error
                    //    // not using the "Exists" method because it throws an (internal) exception too
                    //    client.CreateDirectory(remoteFolder);
                    //}
                    //catch (Exception) { }
                    //
                    //var state = ConsoleManager.GetConsoleState();
                    //_cursorTop = state.Top;
                    //ConsoleManager.ClearLine(state.Top);

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
                        //_cursorTop++;
                        //ConsoleManager.SetConsoleState(0, _cursorTop);
                    }
                    catch (Exception err)
                    {
                        _lastError = $"Error copying {_progress.CurrentFilename}: {err.Message}";
                        return (true, _lastError);
                    }
                    finally
                    {
                        //state.Top = Console.CursorTop + 1;
                        //ConsoleManager.SetConsoleState(state);
                        client.ErrorOccurred -= Client_ErrorOccurred;
                    }

                    _progress.UpdateDisconnected(0);
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
                        _progress.UpdateConnected(1);

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

                _progress.UpdateDisconnected(0);
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

        public IDictionary<string, string> GetSha1ForTree(string remoteFolder)
        {
            var result = new Dictionary<string, string>();
            if (!RemoteFolderExists(remoteFolder))
            {
                return result;
            }

            var cmd = $"find {remoteFolder} -type f -print0  | xargs -0 sha1sum";
            var output = SshRunCommand(cmd);
            if (string.IsNullOrEmpty(output))
            {
                throw new Exception($"Error getting hashes for the remote folder");
            }

            string line = string.Empty;
            using (var st = new StringReader(output))
            {
                while (!string.IsNullOrEmpty(line = st.ReadLine()))
                {
                    var parts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        var outputportion = output.Substring(Math.Min(output.Length, 500));
                        throw new Exception($"Invalid format while retrieving hashes from remote: {outputportion}");
                    }

                    var key = parts[1].Trim();
                    var value = parts[0].Trim();
                    result[key] = value;
                }
            }

            return result;
        }

        [Obsolete("This API throws internally and is a waste of time. Use RemoteFolderExists instead")]
        public bool RemoteDirectoryExists(string remoteFolder)
        {
            using (var client = new SftpClient(_connectionInfo))
            {
                client.Connect();
                try
                {
                    return client.Exists(remoteFolder);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // === test memo ===
        //-b filename       Block special file
        //-c filename       Special character file
        //-d directoryname  Check for directory Existence
        //-e filename       Check for file existence, regardless of type(node, directory, socket, etc.)
        //-f filename       Check for regular file existence not a directory
        //-G filename       Check if file exists and is owned by effective group ID
        //-G filename set-group-id      True if file exists and is set-group-id
        //-k filename       Sticky bit
        //-L filename       Symbolic link
        //-O filename       True if file exists and is owned by the effective user id
        //-r filename       Check if file is a readable
        //-S filename       Check if file is socket
        //-s filename       Check if file is nonzero size
        //-u filename       Check if file set-user-id bit is set
        //-w filename       Check if file is writable
        //-x filename       Check if file is executable
        public bool RemoteFileExists(string remoteFile, SshClient client = null)
        {
            string cmd = $"test -f {remoteFile} && echo 1 || echo 0";
            var output = SshRunCommand(cmd, client).Trim(_eol);
            if (output == "1")
            {
                return true;
            }
            else if (output == "0")
            {
                return false;
            }

            throw new Exception($"Error checking remote file existance: {output}");
        }

        public bool RemoteFolderExists(string remoteFolder, SshClient client = null)
        {
            string cmd = $"test -d {remoteFolder} && echo 1 || echo 0";
            var output = SshRunCommand(cmd, client).Trim(_eol);
            if (output == "1")
            {
                return true;
            }
            else if (output == "0")
            {
                return false;
            }

            throw new Exception($"Error checking remote file existance: {output}");
        }

        public string SshRunCommand(string remoteCommand, SshClient client = null)
        {
            bool mustDispose = false;
            try
            {
                if (client == null)
                {
                    client = new SshClient(_connectionInfo);
                    mustDispose = true;
                }

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
                if (mustDispose)
                {
                    client.Dispose();
                }
            }
        }

        public (bool isError, int synced) EchoFoldersRecursive(string localFolder, string remoteFolder)
        {
            // TODO
            return (true, 0);
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

        //private void OnTransfer(SshProgress progress)
        //{
        //    ConsoleManager.WriteAt(0, _cursorTop, progress.FormattedString);
        //}

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
