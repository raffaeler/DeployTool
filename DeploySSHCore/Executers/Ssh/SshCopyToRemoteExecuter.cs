﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeploySSH.Configuration;
using DeploySSH.Helpers;

namespace DeploySSH.Executers
{
    public class SshCopyToRemoteExecuter : ExecuterBase
    {
        private SshCopyToRemoteAction _action;
        private int _cursorTop;

        public SshCopyToRemoteExecuter(SshCopyToRemoteAction action)
        {
            _action = action;
        }

        public override void Preview(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;
            var sb = new StringBuilder();

            if (_action.DeleteRemoteFolder)
            {
                var expanded = _action.RemoteFolder.Expand(bag);
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
                    var recurse = _action.Recurse ? "Recursive" : "Not Recursive";
                    sb.AppendLine($"Folder copy {recurse}: {expandedLocal} -> {expandedRemote}");
                }
                else
                {
                    sb.AppendLine($"File copy: {expandedLocal} -> {expandedRemote}");
                }
            }

            bag.IsSuccess = true;
            bag.Output = sb.ToString();
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;

            using (var session = manager.CreateSession())
            {
                if (_action.DeleteRemoteFolder)
                {
                    var expanded = _action.RemoteFolder.Expand(bag);
                    session.SshRemoveRemoteFolderTree(expanded);
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
                        try
                        {
                            var result = session.SshCopyDirectoryToRemote(new DirectoryInfo(expandedLocal), expandedRemote,
                                _action.Recurse, OnProgress, remoteExecutable);
                            bag.IsSuccess = true;
                            bag.Output = result.LastOutput;
                        }
                        catch(Exception err)
                        {
                            bag.IsSuccess = false;
                            bag.Output = err.ToString();
                        }
                    }
                    else
                    {
                        try
                        {
                            var result = session.SshCopyFileToRemote(new FileInfo(expandedLocal), expandedRemote, OnProgress);
                            bag.IsSuccess = true;
                            bag.Output = result.LastOutput;
                        }
                        catch (Exception err)
                        {
                            bag.IsSuccess = false;
                            bag.Output = err.ToString();
                        }
                    }

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