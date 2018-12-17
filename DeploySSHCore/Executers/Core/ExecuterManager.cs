using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Configuration;
using DeploySSH.Helpers;

namespace DeploySSH.Executers
{
    public class ExecuterManager
    {
        public ExecuterManager()
        {
            Bag = new PipelineBag();
        }

        public PipelineBag Bag { get; private set; }

        public int Execute(DeployConfiguration deployConfiguration, bool preview)
        {
            int res = -1;
            try
            {
                var sshManager = new SshManager(deployConfiguration.Ssh);
                Bag.SetValue("ssh", sshManager);

                Console.WriteLine($"Processing {deployConfiguration.Description}");

                foreach (var action in deployConfiguration.Actions)
                {
                    Bag.IsSuccess = null;
                    Bag.Output = string.Empty;

                    Console.WriteLine();
                    Console.WriteLine(action.ActionName);
                    var executer = GetExecuter(action);
                    if (preview)
                    {
                        executer.Preview(Bag);
                    }
                    else
                    {
                        executer.Execute(Bag);
                    }

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
                    //Console.WriteLine("SshCopyToRemoteAction");
                    return new SshCopyToRemoteExecuter(sshCopyToRemoteAction);

                case DotnetPublishAction dotnetPublishAction:
                    //Console.WriteLine("DotnetPublishAction");
                    return new DotNetPublishExecuter(dotnetPublishAction);

                case SshRunCommandAction executeCommandAction:
                    //Console.WriteLine("SshRunCommandAction");
                    return new SshRunCommandExecuter(executeCommandAction);

                case SshRunAppAction executeAppAction:
                    //Console.WriteLine("SshRunAppAction");
                    return new SshRunAppExecuter(executeAppAction);

                case SshSyncRemoteAction syncRemoteAction:
                    return new SshSyncRemoteExecuter(syncRemoteAction);

                default:
                    throw new Exception($"Unexpected action {action.GetType().Name}");
            }
        }
    }
}
