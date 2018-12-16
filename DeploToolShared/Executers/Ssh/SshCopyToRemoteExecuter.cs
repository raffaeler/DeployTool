using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class SshCopyToRemoteExecuter : ExecuterBase
    {
        private SshCopyToRemoteAction _action;
        private int _cursorTop;

        public SshCopyToRemoteExecuter(SshCopyToRemoteAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshTransfer transfer)) return;

            if (_action.DeleteRemoteFolder)
            {
                var expanded = _action.RemoteFolder.Expand(bag);
                transfer.SshRemoveRemoteFolderTree(expanded);
            }

            foreach (var item in _action.LocalItems)
            {
                var expandedLocal = item.Expand(bag);
                var expandedRemote = _action.RemoteFolder.Expand(bag);

                FileAttributes attr = File.GetAttributes(expandedLocal);

                // the name of the remote executable is the same of the project name
                bag.TryGet(PipelineBag.AssemblyName, out string remoteExecutable);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    (bool isError, string output) result;
                    //if (_action.DeleteRemoteFolder)
                    //{
                    //    result = transfer.SshCopyDirectoryToRemote(new DirectoryInfo(expandedLocal), expandedRemote,
                    //    _action.Recurse, remoteExecutable);
                    //}
                    //else
                    {
                        result = transfer.SshCopyDirectoryToRemote2(new DirectoryInfo(expandedLocal), expandedRemote,
                        _action.Recurse, OnProgress, remoteExecutable);
                    }

                    bag.IsSuccess = !result.isError;
                    bag.Output = result.output;
                }
                else
                {
                    var result = transfer.SshCopyFileToRemote(new FileInfo(expandedLocal), expandedRemote, OnProgress);
                    bag.IsSuccess = !result.isError;
                    bag.Output = result.output;
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
