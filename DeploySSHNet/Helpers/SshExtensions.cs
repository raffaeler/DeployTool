using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DeploySSH.Helpers
{
    public static class SshExtensions
    {
        private static char[] _eol = new char[] { '\n', '\r' };

        public static string RunCommand(this SshSession session, string remoteCommand)
        {
            try
            {
                var client = session.SshClientConnected;
                var command = client.CreateCommand($"{remoteCommand}");
                var output = command.Execute();
                return output;
            }
            catch (Exception err)
            {
                Debug.WriteLine($"Error running command {remoteCommand}: {err.ToString()}");
                throw;
            }
        }

        public static string Touch(this SshSession session, string remoteFile)
        {
            if (remoteFile.StartsWith("~/")) remoteFile = remoteFile.Substring(2);
            var cmd = $"touch {remoteFile}";
            return session.RunCommand(cmd);
        }

        [Obsolete("Avoid this call as the underlying library throws when the file does not exists")]
        public static bool RemoteDirectoryExists(this SshSession session, string remoteFolder)
        {
            var client = session.SftpClientConnected;
            try
            {
                return client.Exists(remoteFolder);
            }
            catch (Exception)
            {
                return false;
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
        public static bool RemoteFileExists(this SshSession session, string remoteFile)
        {
            string cmd = $"test -f {remoteFile} && echo 1 || echo 0";
            var output = session.RunCommand(cmd).Trim(_eol);
            if (output == "1")
            {
                return true;
            }
            else if (output == "0")
            {
                return false;
            }

            throw new Exception($"Error on {nameof(RemoteFileExists)}: the command \"{cmd}\" returned \"{output}\"");
        }

        public static bool RemoteFolderExists(this SshSession session, string remoteFolder)
        {
            string cmd = $"test -d {remoteFolder} && echo 1 || echo 0";
            var output = session.RunCommand(cmd).Trim(_eol);
            if (output == "1")
            {
                return true;
            }
            else if (output == "0")
            {
                return false;
            }

            throw new Exception($"Error on {nameof(RemoteFolderExists)}: the command \"{cmd}\" returned \"{output}\"");
        }

        public static string SshRemoteCopy(this SshSession session, string remoteSource, string remoteTarget)
        {
            string cmd = $"cp {remoteSource} {remoteTarget}";
            return session.RunCommand(cmd);
        }

        public static void MakeRemoteExecutable(this SshSession session,
            string remoteFolder, string remoteExecutable)
        {
            if (string.IsNullOrEmpty(remoteExecutable))
            {
                throw new ArgumentException(nameof(remoteExecutable));
            }

            var remoteFullExecutable = $"{remoteFolder.TrimEnd('/')}/{remoteExecutable}";
            var client = session.SftpClientConnected;

            try
            {
                if (session.RemoteFileExists(remoteFullExecutable))
                {
                    client.ChangePermissions(remoteFullExecutable, 755);
                }
            }
            catch (Exception err)
            {
                var fullError = new Exception($"Error assigning the permissions to {remoteFullExecutable}: {err.Message}", err);
                session.SshProgress?.UpdateProgressErrorAborting(fullError);
                throw fullError;
            }
        }

        public static IDictionary<string, string> GetSha1ForTree(this SshSession session, string remoteFolder)
        {
            var result = new Dictionary<string, string>();
            if (!session.RemoteFolderExists(remoteFolder))
            {
                return result;
            }

            var cmd = $"find {remoteFolder} -type f -print0  | xargs -0 sha1sum";
            var output = session.RunCommand(cmd);
            if (string.IsNullOrEmpty(output))
            {
                throw new Exception($"Error getting hashes for the remote folder");
            }

            string line = string.Empty;
            using (var st = new StringReader(output))
            {
                while (!string.IsNullOrEmpty(line = st.ReadLine()))
                {
                    var index = line.IndexOf(' ');
                    if (index == -1) continue;
                    var part1 = line.Substring(0, index);
                    var part2 = line.Substring(index).Trim();
                    if (!part2.StartsWith(remoteFolder)) continue;
                    result[part2] = part1;
                }
            }

            return result;
        }

        public static SshOperationResult SshCopyDirectoryToRemote(this SshSession session,
            DirectoryInfo localFolder, string remoteFolder,
            bool recurse, Action<SshProgress> onTransfer = null,
            string remoteExecutable = null)
        {
            var folderInfo = localFolder.GetSizeAndAmount();
            session.CreateProgress("SshCopyDirectoryToRemote2", folderInfo.size, folderInfo.amount, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            var rf = $"{remoteFolder.TrimEnd('/')}/";
            int count = 0;
            var clicmd = session.SshClientConnected;
            var client = session.SftpClientConnected;

            var walker = new DirectoryWalker(localFolder, recurse);
            if (!session.RemoteFolderExists(remoteFolder))
                client.CreateDirectory(remoteFolder);

            try
            {
                var isSuccess = walker.Walk((fileInfo, relative) =>
                {
                    try
                    {
                        using (var fs = File.OpenRead(fileInfo.FullName))
                        {
                            client.UploadFile(fs, rf + relative, true, length =>
                                session.SshProgress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length));
                        }

                        ++count;
                        return true;
                    }
                    catch (Exception err)
                    {
                        session.SshProgress.UpdateProgressErrorAborting(err);
                        throw;
                    }
                },
                (directoryInfo, relative) =>
                {
                    client.CreateDirectory(rf + relative);
                    return true;
                });

                if (!isSuccess || session.SshProgress.LastError != null)
                    throw session.SshProgress.LastError;
            }
            catch (Exception err)
            {
                var fullError = new Exception($"{nameof(SshCopyDirectoryToRemote)}: error copying {session.SshProgress.CurrentFilename}: {err.Message}", err);
                session.SshProgress.UpdateProgressErrorAborting(fullError);
                throw fullError;
            }

            session.MakeRemoteExecutable(remoteFolder, remoteExecutable);
            return new SshOperationResult(nameof(SshCopyDirectoryToRemote));
        }

        public static bool SshCreateRemoteFolder(this SshSession session, string remoteFolder)
        {
            bool wasCreated = false;
            if (!remoteFolder.StartsWith("/") && !remoteFolder.StartsWith("~"))
            {
                throw new Exception($"{nameof(SshCreateRemoteFolder)}: The remote folder must be absolute. The request was instead: ({remoteFolder})");
            }

            var remoteFolders = remoteFolder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var client = session.SftpClientConnected;

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

                    if (!session.RemoteFolderExists(root))
                    {
                        client.CreateDirectory(root);
                        wasCreated = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception($"{nameof(SshCreateRemoteFolder)}: {err.Message}", err);
            }

            return wasCreated;
        }

        public static SshOperationResult SshCopyFileToRemote(this SshSession session, FileInfo fileInfo,
            string remoteFolder, Action<SshProgress> onTransfer)
        {
            session.CreateProgress("SshCopyFileToRemote", fileInfo.Length, 1, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);
            var rf = $"{remoteFolder.TrimEnd('/')}/";
            var client = session.SftpClientConnected;

            session.SshCreateRemoteFolder(remoteFolder);

            try
            {
                using (var fs = File.OpenRead(fileInfo.FullName))
                {
                    client.UploadFile(fs, rf + fileInfo.Name, true, length =>
                        session.SshProgress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length));
                }

                if (session.SshProgress.LastError != null)
                    throw session.SshProgress.LastError;
            }
            catch (Exception err)
            {
                session.SshProgress.UpdateProgressErrorAborting(err);
                throw new Exception($"{nameof(SshCopyFileToRemote)}: {err.Message}", err);
            }


            return new SshOperationResult(nameof(SshCopyFileToRemote));
        }

        public static bool SshDeleteRemoteFile(this SshSession session,
            string remoteFile)
        {
            var client = session.SftpClientConnected;
            if (remoteFile.StartsWith("~/")) remoteFile = remoteFile.Substring(2);

            try
            {
                if (!session.RemoteFileExists(remoteFile))
                {
                    return false;
                }

                client.DeleteFile(remoteFile);
                return true;
            }
            catch (Exception err)
            {
                session.SshProgress.UpdateProgressErrorAborting(err);
                throw new Exception($"{nameof(SshDeleteRemoteFile)}: {err.Message}", err);
            }
        }

        public static bool SshRemoveRemoteFolderTree(this SshSession session, string remoteFolder)
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

            var client = session.SshClientConnected;

            try
            {
                var command = client.CreateCommand($"rm -rf {remoteFolder}");
                var output1 = command.Execute();
                var output = command.Result;
                if (!string.IsNullOrEmpty(command.Error))
                {
                    throw new Exception(command.Error);
                }

                return true;
            }
            catch (Exception err)
            {
                throw new Exception($"{nameof(SshRemoveRemoteFolderTree)}: error on the delete command: {err.ToString()}");
            }
        }

        /// <summary>
        /// This method Echo/Mirror a Windows folder to the remote side (Linux or Windows)
        /// First the complete list of hashes for the existent (if any) files on the remote
        /// side is retrieved. See the Sha1 method docs.
        /// The local file system is walked recursively finding files and folders.
        /// The folders are created if they do not exists, but they are not removed on the
        /// remote side since the hash dictionary only contains files but not folders.
        /// The files are processed depending whether they exists or not on the remote side.
        /// If a file already exist, the hash is computed locally. If they match, the file is
        /// skipped, otherwise it is overwritten.
        /// If the file does not exist, it is copied on the remote side.
        /// At the end the files on the remote side that had no match with the local side
        /// are processed. Depending on a flag they are removed or not.
        /// A summary report is returned to the caller
        /// </summary>
        /// <param name="session">The ssh session to use</param>
        /// <param name="localFolder">The local folder</param>
        /// <param name="remoteFolder">The remote folder (absolute or relative to user's home)</param>
        /// <param name="onTransfer">The detailed callback</param>
        /// <param name="onRemoving">A callback asking confirm before removing a file on the remote side.
        /// Passing null will remove all files</param>
        /// <returns></returns>
        public static SshSyncResult EchoFoldersRecursive(this SshSession session,
            DirectoryInfo localFolder, string remoteFolder, bool recurse = true,
            Action<SshProgress> onTransfer = null,
            Func<string, bool> onRemoving = null)
        {
            var result = new SshSyncResult();
            var folderInfo = localFolder.GetSizeAndAmount();
            session.CreateProgress("EchoFoldersRecursive", folderInfo.size, folderInfo.amount, onTransfer);
            //bool wasCreated = SshCreateRemoteFolder(remoteFolder);

            var rf = $"{remoteFolder.TrimEnd('/')}/";
            if (rf.StartsWith("~/")) rf = rf.Substring(2);

            var clicmd = session.SshClientConnected;
            var client = session.SftpClientConnected;

            var remoteHashes = session.GetSha1ForTree(rf);

            var walker = new DirectoryWalker(localFolder, recurse);
            if (!session.RemoteFolderExists(remoteFolder))
                session.SshCreateRemoteFolder(remoteFolder);

            try
            {
                var isSuccess = walker.Walk(OnFile, OnDirectory);

                if (!isSuccess || session.SshProgress.LastError != null)
                {
                    result.Success = false;
                    return result;
                }

                session.SshProgress.UpdateProgressFinal(result.Total);
            }
            catch (Exception err)
            {
                var fullError = $"Error copying {session.SshProgress.CurrentFilename}: {err.Message}";
                session.SshProgress.UpdateProgressErrorAborting(new Exception(fullError, err));
                result.Success = false;
                return result;
            }

            // the files existing on the remote side are these ones:
            var remaining = remoteHashes.Keys;
            foreach(var file in remaining)
            {
                // returning null or true will remove the file
                var toremove = onRemoving?.Invoke(file);
                if (!toremove.HasValue || toremove.Value)
                {
                    result.Removed++;
                    client.DeleteFile(file);
                }
            }

            // TODO
            result.Success = true;
            return result;

            bool OnFile(FileInfo fileInfo, string relative)
            {
                try
                {
                    var length = fileInfo.Length;
                    var remoteFile = rf + relative;
                    bool existed = false;
                    if (remoteHashes.TryGetValue(remoteFile, out string remoteHash))
                    {
                        remoteHashes.Remove(remoteFile);

                        // the file already exists on the remote side
                        // we need to verify the hash
                        var localHash = HashHelper.CreateSHA1String(fileInfo.FullName);

                        // if the hashes match, there is no need to copy the file
                        if (localHash == remoteHash)
                        {
                            result.Skipped++;
                            session.SshProgress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)length);
                            return true;
                        }

                        existed = true;
                    }

                    using (var fs = File.OpenRead(fileInfo.FullName))
                    {
                        client.UploadFile(fs, rf + relative, true, len =>
                            session.SshProgress.UpdateProgress(fileInfo.Name, fileInfo.Length, (long)len));
                    }

                    if (existed)
                        result.CopiedOver++;
                    else
                        result.CopiedNew++;
                    return true;
                }
                catch (Exception err)
                {
                    session.SshProgress.UpdateProgressErrorAborting(err);
                    return false;
                }
            }

            bool OnDirectory(DirectoryInfo directoryInfo, string relative)
            {
                if(!session.RemoteFolderExists(rf + relative))
                    client.CreateDirectory(rf + relative);
                return true;
            }
        }

    }
}
