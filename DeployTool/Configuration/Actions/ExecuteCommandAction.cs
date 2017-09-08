using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class ExecuteCommandAction : IAction
    {
        public ExecuteCommandAction()
        {
            ActionName = this.GetType().Name;
        }

        public string ActionName { get; set; }
        public string Command { get; set; }
    }
}
