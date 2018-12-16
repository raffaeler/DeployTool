using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DeployTool.Helpers
{
    public static class DPApiHelper
    {
        private static byte[] _optionalEntropy = { 0x0c, 0x06, 0xf, 0x01, 0x03 };

        public static string Encrypt(string clearText)
        {
            var bytes = Encoding.UTF8.GetBytes(clearText);
            var encrypted = Protect(bytes);
            var readable = GetTextFromBlob(encrypted);
            return readable;
        }

        public static string Decrypt(string encryptedText)
        {
            var bytes = GetBlobFromText(encryptedText);
            var clearTextData = Unprotect(bytes);
            var clearText = Encoding.UTF8.GetString(clearTextData);
            return clearText;
        }

        private static byte[] GetBlobFromText(string data)
        {
            int len = data.Length;
            if (len % 2 != 0)
            {
                throw new Exception("Invalid string provided: ensure it is a valid encrypted hex string");
            }

            try
            {
                byte[] bytes = new byte[len / 2];
                int index = 0;
                for (int i = 0; i < len; i += 2)
                {
                    bytes[index] = Convert.ToByte(data.Substring(i, 2), 16);
                    index++;
                }

                return bytes;
            }
            catch (Exception err)
            {
                throw new Exception($"There are invalid (non-hex) characters in the string: {err.Message}");
            }
        }

        private static string GetTextFromBlob(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }

        private static byte[] Protect(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, _optionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (Exception e)
            {
                throw new Exception($"Encryption failed: {e.Message}");
            }
        }

        private static byte[] Unprotect(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, _optionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (Exception e)
            {
                throw new Exception($"Decryption failed: {e.Message}\r\n");
            }
        }

    }
}
