using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class CopyToRemoteExecuter : ExecuterBase
    {
        private CopyToRemoteAction _action;
        public CopyToRemoteExecuter(CopyToRemoteAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshConfiguration ssh)) return;
            var transfer = new SshTransfer(ssh);

            if (_action.DeleteRemoteFolder)
            {
                transfer.RemoveRemoteFolderTree(_action.RemoteFolder);
            }

            foreach (var item in _action.LocalItems)
            {
                FileAttributes attr = File.GetAttributes(item);

                // the name of the remote executable is the same of the project name
                bag.TryGet("projectName", out string remoteExecutable);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    transfer.CopyDirectoryToRemote(new DirectoryInfo(item), _action.RemoteFolder,
                        _action.Recurse, remoteExecutable);
                }
                else
                {
                    transfer.CopyFileToRemote(new FileInfo(item), _action.RemoteFolder);
                }
            }

        }
    }
}
