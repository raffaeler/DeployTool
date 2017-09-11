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
            if (!bag.GetSshOrFail(out SshTransfer transfer)) return;

            var expanded = _action.Command.Expand(bag);

            var output = transfer.SshRunCommand(expanded);
            bag.SetSuccess(output);
        }
    }
}
