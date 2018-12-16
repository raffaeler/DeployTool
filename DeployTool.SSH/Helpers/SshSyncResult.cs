using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Helpers
{
    public class SshSyncResult
    {
        public SshSyncResult()
        {
        }

        public SshSyncResult(bool success, int copiedNew, int copiedOver, int skipped, int removed)
        {
            Success = success;
            CopiedNew = copiedNew;
            CopiedOver = copiedOver;
            Skipped = skipped;
            Removed = removed;
        }

        public bool Success { get; internal set; }
        public int CopiedNew { get; internal set; }
        public int CopiedOver { get; internal set; }
        public int Skipped { get; internal set; }
        public int Removed { get; internal set; }

        public int Total => CopiedNew + CopiedOver + Skipped;
    }
}
