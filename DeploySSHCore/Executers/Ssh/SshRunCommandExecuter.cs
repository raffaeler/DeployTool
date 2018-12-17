using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Configuration;
using DeploySSH.Helpers;

namespace DeploySSH.Executers
{
    public class SshRunCommandExecuter : ExecuterBase
    {
        private SshRunCommandAction _action;
        public SshRunCommandExecuter(SshRunCommandAction action)
        {
            _action = action;
        }

        public override void Preview(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;
            var expanded = _action.Command.Expand(bag);
            bag.SetSuccess(expanded);
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;

            using (var session = manager.CreateSession())
            {
                var expanded = _action.Command.Expand(bag);

                var output = session.RunCommand(expanded);
                bag.SetSuccess(output);
            }
        }
    }
}
