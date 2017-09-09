using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class DeployConfiguration
    {
        /// <summary>
        /// The description of the configuration (optional)
        /// </summary>
        public string Description { get; set; }

        public SshConfiguration Ssh { get; set; }

        public IList<IAction> Actions { get; set; }
    }
}
