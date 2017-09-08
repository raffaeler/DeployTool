using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;

namespace DeployTool.Executers
{
    public abstract class ExecuterBase
    {
        public ExecuterBase()
        {
        }

        public abstract void Execute(PipelineBag bag);
    }
}
