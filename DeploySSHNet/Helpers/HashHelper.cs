using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DeploySSH.Helpers
{
    public static class HashHelper
    {
        public static byte[] CreateSHA1(string filename)
        {
            using (var fs = File.OpenRead(filename))
            {
                using (var bs = new BufferedStream(fs))
                {
                    using (var sha1 = new SHA1Managed())
                    {
                        byte[] hash = sha1.ComputeHash(bs);
                        StringBuilder formatted = new StringBuilder(2 * hash.Length);
                        return hash;
                    }
                }
            }
        }

        public static string CreateSHA1String(string filename)
        {
            var hash = CreateSHA1(filename);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
