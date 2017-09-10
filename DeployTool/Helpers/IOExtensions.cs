using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace DeployTool.Helpers
{
    public static class IOExtensions
    {
        public static long GetSize(this FileInfo fileInfo)
        {
            return fileInfo.Length;
        }

        public static long GetSize(this DirectoryInfo directoryInfo)
        {
            long total = 0;
            var walker = new DirectoryWalker(directoryInfo, true, (f, r) =>
            {
                total += f.GetSize();
            });

            return total;
        }

        public static (long size, long amount) GetSizeAndAmount(this DirectoryInfo directoryInfo)
        {
            long total = 0;
            long num = 0;
            var walker = new DirectoryWalker(directoryInfo, true, (f, r) =>
            {
                num++;
                total += f.GetSize();
            });

            return (total, num);
        }
    }
}
