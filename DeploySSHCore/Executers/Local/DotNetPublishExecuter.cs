using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeploySSH.Configuration;
using DeploySSH.Helpers;

namespace DeploySSH.Executers
{
    public class DotNetPublishExecuter : DotNetExecuter
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
            var projectName = PipelineBag.ProjectName.Expand(bag);
            var command = _action.GetDotnetCommand().Expand(bag);
            string output = base.ExecuteAndWait(command, out bool isError);
            bag.SetResult(isError, output);

            if (isError)
            {
                return;
            }

            var infos = FindFolders(output, projectName);
            if (infos.Length == 0)
            {
                bag.SetError(output);
                return;
            }

            var info = infos.Single();

            bag.SetValue(PipelineBag.ProjectName, info.project);
            bag.SetValue(PipelineBag.PublishDir, info.folder);
        }

        private (string project, string folder)[] FindFolders(string output, string projectName)
        {
            var collection = Regex.Matches(output, ResultExpr, RegexOptions.Multiline).OfType<Match>();
            var result = collection
                .Where(x => x.Groups.Count >= MaxGroup && x.Groups[ProjectGroup].Success && x.Groups[FolderGroup].Success)
                .Select(x => (x.Groups[ProjectGroup].Value, x.Groups[FolderGroup].Value))
                .Where(t => string.Compare(t.Item1, projectName, true) == 0 && t.Item2.EndsWith(@"\"))
                .ToArray();

            return result;
        }
    }
}
