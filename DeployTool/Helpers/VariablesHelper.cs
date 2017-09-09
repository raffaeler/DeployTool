using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Executers;

namespace DeployTool.Helpers
{
    public static class VariablesHelper
    {
        public static string Expand(this string value, PipelineBag bag)
        {
            return bag.Expand(value);
        }
    }
}
