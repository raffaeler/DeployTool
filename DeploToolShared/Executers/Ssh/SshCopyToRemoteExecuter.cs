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
                    var result = transfer.SshCopyDirectoryToRemote(new DirectoryInfo(expandedLocal), expandedRemote,
                        _action.Recurse, remoteExecutable);
                    bag.IsSuccess = !result.isError;
                    bag.Output = result.output;
                }
                else
                {
                    var result = transfer.SshCopyFileToRemote(new FileInfo(expandedLocal), expandedRemote);
                    bag.IsSuccess = !result.isError;
                    bag.Output = result.output;
                }

            }

        }
    }
}
