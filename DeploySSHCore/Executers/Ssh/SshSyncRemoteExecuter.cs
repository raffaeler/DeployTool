using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeploySSH.Configuration;
using DeploySSH.Helpers;

namespace DeploySSH.Executers
{
    public class SshSyncRemoteExecuter : ExecuterBase
    {
        private SshSyncRemoteAction _action;
        private int _cursorTop;

        public SshSyncRemoteExecuter(SshSyncRemoteAction action)
        {
            _action = action;
        }

        public override void Preview(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;
            var sb = new StringBuilder();

            var expandedLocal = _action.LocalFolder.Expand(bag);
            var expandedRemote = _action.RemoteFolder.Expand(bag);
            var diLocal = new DirectoryInfo(expandedLocal);

            FileAttributes attr = File.GetAttributes(expandedLocal);

            // the name of the remote executable is the same of the project name
            bag.TryGet(PipelineBag.AssemblyName, out string remoteExecutable);

            sb.AppendLine($"Sync-Echo: {expandedLocal} -> {expandedRemote}");
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                sb.AppendLine($"Warning: the specified local folder does not (still) exist");
            }
            else
            {
                var fileStatistics = GetFileStatistics(diLocal, _action.Recurse, _action.DeleteRemoteOrphans);
                sb.AppendLine($"As much as {fileStatistics} files will be copied to the remote side");
                if (_action.Recurse)
                {
                    Console.WriteLine("Action is recursive");
                    var folderStatistics = GetFoldersStatistics(diLocal, _action.Recurse, _action.DeleteRemoteOrphans);
                    sb.AppendLine($"As much as {folderStatistics} folder will be copied to the remote side");
                    var folders = string.Join(", ", diLocal.GetDirectories().Select(d => d.Name));
                    sb.AppendLine($"The included sub-folders are:\r\n{folders}");
                }
            }

            if (_action.DeleteRemoteOrphans)
            {
                if (_action.Recurse)
                {
                    sb.AppendLine($"Warning: the remote folder AND subfolders are subject to deletion");
                }
                else
                {
                    sb.AppendLine($"Warning: the remote folder (but not subfolders) are subject to deletion");
                }
            }

            if (!string.IsNullOrEmpty(remoteExecutable))
            {
                sb.AppendLine($"Giving execution permission to {remoteExecutable} in the {expandedRemote} folder");
            }

            bag.SetSuccess(sb.ToString());
        }

        private int GetFileStatistics(DirectoryInfo diLocal, bool recurse, bool deleteRemoteOrphans)
        {
            var localFiles = diLocal.GetFiles("*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return localFiles.Length;
        }

        private int GetFoldersStatistics(DirectoryInfo diLocal, bool recurse, bool deleteRemoteOrphans)
        {
            var folders = diLocal.GetDirectories("*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return folders.Length;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;

            using (var session = manager.CreateSession())
            {
                var expandedLocal = _action.LocalFolder.Expand(bag);
                var expandedRemote = _action.RemoteFolder.Expand(bag);

                FileAttributes attr = File.GetAttributes(expandedLocal);

                // the name of the remote executable is the same of the project name
                bag.TryGet(PipelineBag.AssemblyName, out string remoteExecutable);

                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    bag.IsSuccess = false;
                    bag.Output = $"The \"LocalFolder\" {_action.LocalFolder} must be a folder and exist";
                    return;
                }

                try
                {
                    var result = session.EchoFoldersRecursive(new DirectoryInfo(expandedLocal), expandedRemote,
                        _action.Recurse, OnProgress, _ => _action.DeleteRemoteOrphans);
                    bag.IsSuccess = result.Success;
                    bag.Output = result.ToString();
                }
                catch (Exception err)
                {
                    bag.IsSuccess = false;
                    bag.Output = err.ToString();
                }

                if (!string.IsNullOrEmpty(remoteExecutable))
                {
                    session.MakeRemoteExecutable(expandedRemote, remoteExecutable);
                }
            }
        }

        private void OnProgress(SshProgress sshProgress)
        {
            var context = sshProgress.OperationContext;
            switch (sshProgress.SshTransferStatus)
            {
                case SshTransferStatus.Starting:
                    break;
                case SshTransferStatus.Connected:
                    var state = ConsoleManager.GetConsoleState();
                    _cursorTop = state.Top;
                    ConsoleManager.ClearLine(state.Top);
                    break;
                case SshTransferStatus.UpdateProgress:
                    _cursorTop++;
                    ConsoleManager.SetConsoleState(0, _cursorTop);
                    break;
                case SshTransferStatus.Completed:
                    break;
                case SshTransferStatus.Disconnected:
                    ConsoleManager.WriteSuccess($"{context}: Success!");
                    break;
                case SshTransferStatus.ErrorAborting:
                    ConsoleManager.WriteError($"{context} Error: {sshProgress.LastError.ToString()}");
                    break;
            }
        }
    }
}
