using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class SshConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public PrivateKeyData[] PrivateKeys { get; set; }

        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }

    }
}
