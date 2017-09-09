using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class ExecuterManager
    {
        public ExecuterManager()
        {
            Bag = new PipelineBag();
        }

        public PipelineBag Bag { get; private set; }

        public void Execute(DeployConfiguration deployConfiguration)
        {
            SshTransfer sshTransfer = new SshTransfer(deployConfiguration.Ssh);
            Bag.SetValue("ssh", sshTransfer);

            Console.WriteLine($"Processing {deployConfiguration.Description}");

            foreach (var action in deployConfiguration.Actions)
            {
                var executer = GetExecuter(action);
                executer.Execute(Bag);
                if (!Bag.IsSuccess)
                {

                    WriteError($"Error: {Bag.Output}");
                    break;
                }
            }
        }

        private void WriteError(string message)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = current;
        }

        private ExecuterBase GetExecuter(IAction action)
        {
            switch (action)
            {
                case SshCopyToRemoteAction sshCopyToRemoteAction:
                    return new SshCopyToRemoteExecuter(sshCopyToRemoteAction);

                case DotnetPublishAction dotnetPublishAction:
                    return new DotNetPublishExecuter(dotnetPublishAction);

                case SshRunCommandAction executeCommandAction:
                    return new SshRunCommandExecuter(executeCommandAction);

                case SshRunAppAction executeAppAction:
                    return new SshRunAppExecuter(executeAppAction);

                default:
                    throw new Exception($"Unexpected action {action.GetType().Name}");
            }
        }
    }
}
