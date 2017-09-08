using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class CopyToRemoteAction : IAction
    {
        public CopyToRemoteAction()
        {
            ActionName = this.GetType().Name;
        }

        public string ActionName { get; }
        public string[] LocalItems { get; set; }
        public string RemoteFolder { get; set; }
        //public bool Overwrite { get; set; }
        public bool DeleteRemoteFolder { get; set; }
        public bool Recurse { get; set; }
    }
}
