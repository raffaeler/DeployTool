using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Configuration
{
    public class SshSyncRemoteAction : IAction
    {
        public SshSyncRemoteAction()
        {
            ActionName = this.GetType().Name;
        }

        public string GetShortActionName() => "SshSyncRemote";

        public string ActionName { get; }
        public string LocalFolder { get; set; }
        public string RemoteFolder { get; set; }
        public bool DeleteRemoteOrphans { get; set; }
        public bool Recurse { get; set; }
    }
}
