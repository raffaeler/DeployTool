using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class SshRunAppExecuter : ExecuterBase
    {
        private SshRunAppAction _action;
        public SshRunAppExecuter(SshRunAppAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.GetSshOrFail(out SshManager manager)) return;

            using (var session = manager.CreateSession())
            {
                var command = $"{_action.RemoteFolder}/{_action.RemoteApp} {_action.Arguments}";
                var expanded = command.Expand(bag);

                var output = session.RunCommand(expanded);
                bag.SetSuccess(output);
            }
        }
    }
}
