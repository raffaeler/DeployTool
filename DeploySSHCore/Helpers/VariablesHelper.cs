using System;
using System.Collections.Generic;
using System.Text;
using DeploySSH.Executers;

namespace DeploySSH.Helpers
{
    public static class VariablesHelper
    {
        public static string Expand(this string value, PipelineBag bag, bool failIfNotFound = true)
        {
            return bag.Expand(value, failIfNotFound);
        }
    }
}
