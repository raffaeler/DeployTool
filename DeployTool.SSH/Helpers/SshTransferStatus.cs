using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Helpers
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
