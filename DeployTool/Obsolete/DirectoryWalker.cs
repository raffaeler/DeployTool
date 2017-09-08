using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeployTool.Helpers
{
    public class DirectoryWalker
    {
        private Action<FileInfo, string> OnFileAction { get; set; }

        private StringBuilder RelativePath { get; set; }

        public DirectoryWalker(DirectoryInfo directoryInfo,
            bool recurse, Action<FileInfo, string> onFileAction)
        {
            this.OnFileAction = onFileAction;
            RelativePath = new StringBuilder();
            Walk(directoryInfo, "", recurse);
        }


        private void Walk(DirectoryInfo directoryInfo, string relativePath, bool recurse)
        {
            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                var relative = relativePath.Length == 0 ? fileInfo.Name : $"{relativePath}/{fileInfo.Name}";
                //var relative = relativePath;
                Walk(fileInfo, relative);
            }

            if (recurse)
            {
                foreach (var subdir in directoryInfo.EnumerateDirectories())
                {
                    var relative = relativePath.Length == 0 ? subdir.Name : $"{relativePath}/{subdir.Name}";
                    Walk(subdir, relative, recurse);
                }
            }
        }

        private void Walk(FileInfo fileInfo, string relativePath)
        {
            OnFileAction(fileInfo, relativePath);
        }
    }
}
