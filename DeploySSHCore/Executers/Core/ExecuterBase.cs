using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Configuration;

namespace DeploySSH.Executers
{
    public abstract class ExecuterBase
    {
        public ExecuterBase()
        {
        }

        public abstract void Execute(PipelineBag bag);
        public abstract void Preview(PipelineBag bag);
    }
}
