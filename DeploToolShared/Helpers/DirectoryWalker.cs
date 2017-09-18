using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeployTool.Helpers
{
    public class DirectoryWalker
    {
        private Func<FileInfo, string, bool> OnFileAction { get; set; }
        private Func<DirectoryInfo, string, bool> OnDirectoryAction { get; set; }

        private StringBuilder RelativePath { get; set; }

        private bool _recurse;

        private DirectoryInfo _directoryInfo;

        private bool _shouldContinue;

        public DirectoryWalker(DirectoryInfo directoryInfo,
            bool recurse)
        {
            _directoryInfo = directoryInfo;
            _recurse = recurse;
        }

        public bool Walk(Func<FileInfo, string, bool> onFileAction, Func<DirectoryInfo, string, bool> onDirectoryAction = null)
        {
            _shouldContinue = true;
            this.OnFileAction = onFileAction;
            this.OnDirectoryAction = onDirectoryAction;
            RelativePath = new StringBuilder();
            Walk(_directoryInfo, "", _recurse);
            return _shouldContinue;
        }

        private void Walk(DirectoryInfo directoryInfo, string relativePath, bool recurse)
        {
            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                if (!_shouldContinue) return;

                var relative = relativePath.Length == 0 ? fileInfo.Name : $"{relativePath}/{fileInfo.Name}";
                //var relative = relativePath;
                _shouldContinue = Notify(fileInfo, relative);
            }

            if (!_shouldContinue) return;

            if (recurse)
            {
                foreach (var subdir in directoryInfo.EnumerateDirectories())
                {
                    var relative = relativePath.Length == 0 ? subdir.Name : $"{relativePath}/{subdir.Name}";
                    if (Notify(subdir, relative))
                    {
                        Walk(subdir, relative, recurse);
                    }
                }
            }
        }

        private bool Notify(FileInfo fileInfo, string relativePath)
        {
            return OnFileAction(fileInfo, relativePath);
        }

        private bool Notify(DirectoryInfo directoryInfo, string relativePath)
        {
            if (OnDirectoryAction != null)
            {
                return OnDirectoryAction(directoryInfo, relativePath);
            }

            return true;
        }
    }
}
