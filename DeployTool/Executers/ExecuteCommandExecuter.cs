using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class ExecuteCommandExecuter : ExecuterBase
    {
        private ExecuteCommandAction _action;
        public ExecuteCommandExecuter(ExecuteCommandAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshConfiguration ssh)) return;
            var transfer = new SshTransfer(ssh);

            var output = transfer.ExecuteRemoteCommand(_action.Command);
            bag.SetSuccess(output);
        }
    }
}
