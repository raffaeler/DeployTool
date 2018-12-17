using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Configuration
{
    public class SshRunAppAction : IAction
    {
        public SshRunAppAction()
        {
            ActionName = this.GetType().Name;
        }

        public string GetShortActionName() => "SshRunApp";

        public string ActionName { get; set; }
        public string RemoteFolder { get; set; }
        public string RemoteApp { get; set; }
        public string Arguments { get; set; }
    }
}
