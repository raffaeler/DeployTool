using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class ExecuteRemoteAppExecuter : ExecuterBase
    {
        private ExecuteRemoteAppAction _action;
        public ExecuteRemoteAppExecuter(ExecuteRemoteAppAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshConfiguration ssh)) return;
            var transfer = new SshTransfer(ssh);

            var command = $"{_action.RemoteFolder}/{_action.RemoteApp} {_action.Arguments}";
            var output = transfer.ExecuteRemoteCommand(command);
            bag.SetSuccess(output);
        }
    }
}
