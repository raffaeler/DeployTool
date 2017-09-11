using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class SshRunCommandAction : IAction
    {
        public SshRunCommandAction()
        {
            ActionName = this.GetType().Name;
        }

        public string GetShortActionName() => "SshRunCommand";

        public string ActionName { get; set; }
        public string Command { get; set; }
    }
}
