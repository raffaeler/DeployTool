using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeployTool.Configuration;

namespace DeployTool.Executers
{
    internal class DotNetPublishExecuter : DotNetExecuter
    {
        private static readonly string ResultExpr = @"^\s+(?<1>\w+)\s+->\s+(?<2>.+)\r";
        private static readonly int ProjectGroup = 1;
        private static readonly int FolderGroup = 2;
        private static readonly int MaxGroup = 2;
        private DotnetPublishAction _action;

        public DotNetPublishExecuter(DotnetPublishAction action)
        {
            _action = action;
        }

        public override void Execute(PipelineBag bag)
        {
            string output = base.ExecuteAndWait(_action.GetDotnetCommand(), out bool isError);

            if (isError)
            {
                bag.SetError(output);
                return;
            }

            var infos = FindFolders(output);
            if (infos.Length == 0)
            {
                bag.SetError(output);
                return;
            }

            var info = infos.Single();

            bag.SetValue("projectName", info.project);
            bag.SetValue("projectFolder", info.folder);
        }

        private (string project, string folder)[] FindFolders(string output)
        {
            var collection = Regex.Matches(output, ResultExpr, RegexOptions.Multiline);
            var result = collection
                .Where(x => x.Groups.Count >= MaxGroup && x.Groups[ProjectGroup].Success && x.Groups[FolderGroup].Success)
                .Select(x => (x.Groups[ProjectGroup].Value, x.Groups[FolderGroup].Value))
                .Where(t => t.Item2.EndsWith(@"\"))
                .ToArray();

            return result;
        }
    }
}
