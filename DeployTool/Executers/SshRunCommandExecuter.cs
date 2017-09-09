using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class SshRunCommandExecuter : ExecuterBase
    {
        private SshRunCommandAction _action;
        public SshRunCommandExecuter(SshRunCommandAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshConfiguration ssh)) return;
            var transfer = new SshTransfer(ssh);

            var output = transfer.SshRunCommand(_action.Command);
            bag.SetSuccess(output);
        }
    }
}
