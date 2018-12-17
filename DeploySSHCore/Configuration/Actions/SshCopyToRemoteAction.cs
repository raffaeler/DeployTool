using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Configuration
{
    public class SshCopyToRemoteAction : IAction
    {
        public SshCopyToRemoteAction()
        {
            ActionName = this.GetType().Name;
        }

        public string GetShortActionName() => "SshCopyToRemote";

        public string ActionName { get; }
        public string[] LocalItems { get; set; }
        public string RemoteFolder { get; set; }
        //public bool Overwrite { get; set; }
        public bool DeleteRemoteFolder { get; set; }
        public bool Recurse { get; set; }
    }
}
