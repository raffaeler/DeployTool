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
                Console.WriteLine();
                Console.WriteLine(action.ActionName);
                var executer = GetExecuter(action);
                executer.Execute(Bag);
                if (!Bag.IsSuccess.HasValue)
                {
                    WriteUnkOutput(Bag.Output);
                    continue;
                }

                if (!Bag.IsSuccess.Value)
                {

                    WriteError($"Error: {Bag.Output}");
                    break;
                }

                WriteSuccess(Bag.Output);
            }

            Console.WriteLine();
        }

        private void WriteSuccess(string message)
        {
            var currentFore = Console.ForegroundColor;
            var currentBack = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ForegroundColor = currentFore;
            Console.BackgroundColor = currentBack;
        }

        private void WriteUnkOutput(string message)
        {
            var currentFore = Console.ForegroundColor;
            var currentBack = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = currentFore;
            Console.BackgroundColor = currentBack;
        }

        private void WriteError(string message)
        {
            var currentFore = Console.ForegroundColor;
            var currentBack = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = currentFore;
            Console.BackgroundColor = currentBack;
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
