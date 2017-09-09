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
        public SshCopyToRemoteExecuter(SshCopyToRemoteAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshConfiguration ssh)) return;
            var transfer = new SshTransfer(ssh);

            if (_action.DeleteRemoteFolder)
            {
                transfer.SshRemoveRemoteFolderTree(_action.RemoteFolder);
            }

            foreach (var item in _action.LocalItems)
            {
                FileAttributes attr = File.GetAttributes(item);

                // the name of the remote executable is the same of the project name
                bag.TryGet("projectName", out string remoteExecutable);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    transfer.SshCopyDirectoryToRemote(new DirectoryInfo(item), _action.RemoteFolder,
                        _action.Recurse, remoteExecutable);
                }
                else
                {
                    transfer.SshCopyFileToRemote(new FileInfo(item), _action.RemoteFolder);
                }
            }

        }
    }
}
