using System;
using System.Collections.Generic;
using System.Text;

namespace DeploySSH.Helpers
{
    public enum SshTransferStatus
    {
        Connected,
        Starting,
        UpdateProgress,
        Completed,
        Disconnected,
        ErrorAborting,
    }
}
