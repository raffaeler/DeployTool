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

        public int Execute(DeployConfiguration deployConfiguration)
        {
            int res = -1;
            try
            {
                SshTransfer sshTransfer = new SshTransfer(deployConfiguration.Ssh);
                Bag.SetValue("ssh", sshTransfer);

                Console.WriteLine($"Processing {deployConfiguration.Description}");

                foreach (var action in deployConfiguration.Actions)
                {
                    Bag.IsSuccess = null;
                    Bag.Output = string.Empty;

                    Console.WriteLine();
                    Console.WriteLine(action.ActionName);
                    var executer = GetExecuter(action);
                    executer.Execute(Bag);
                    if (!Bag.IsSuccess.HasValue)
                    {
                        ConsoleManager.WriteUnkOutput(Bag.Output);
                        continue;
                    }

                    if (!Bag.IsSuccess.Value)
                    {
                        ConsoleManager.WriteError($"Error: {Bag.Output}");
                        break;
                    }

                    ConsoleManager.WriteSuccess(Bag.Output);
                }


                if (!Bag.IsSuccess.HasValue)
                    res = 2;

                if (Bag.IsSuccess.Value)
                {
                    res = 1;
                }
            }
            catch (Exception err)
            {
                ConsoleManager.WriteError(err.Message);
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
            return res;
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
