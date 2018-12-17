using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Helpers
{
    public struct SshOperationResult
    {
        public SshOperationResult(string operationContext, string lastOutput = "")
        {
            OperationContext = operationContext;
            LastOutput = lastOutput;
        }

        public string OperationContext { get; private set; }
        public string LastOutput { get; private set; }
    }
}
