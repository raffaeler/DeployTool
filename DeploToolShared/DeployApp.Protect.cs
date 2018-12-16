using System;
using System.Collections.Generic;
using System.Text;

using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;

namespace DeployTool
{
    internal partial class DeployApp
    {
        private int ProcessProtectCommand(ProtectCommand protectCommand)
        {
            if (string.IsNullOrEmpty(protectCommand.Data))
            {
                Console.WriteLine("The provided string is empty");
                return -1;
            }

            try
            {
                if (protectCommand.IsClear)
                {
                    var encrypted = DPApiHelper.Encrypt(protectCommand.Data);
                    Console.WriteLine($"Protect values are valid *only* for the current user profile");
                    Console.WriteLine($"Write the encrypted value in the configuration");
                    Console.WriteLine();
                    Console.WriteLine($"Clear text:  \"{protectCommand.Data}\"");
                    Console.WriteLine($"Encrypted :  \"{encrypted}\"");
                }
                else
                {
                    var decrypted = DPApiHelper.Decrypt(protectCommand.Data);
                    Console.WriteLine($"Decrypting values are valid *only* for the user profile they were encrypted");
                    Console.WriteLine();
                    Console.WriteLine($"Encrypted :  \"{protectCommand.Data}\"");
                    Console.WriteLine($"Clear text:  \"{decrypted}\"");
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                if (protectCommand.IsClear)
                {
                }
                else
                {
                    Console.WriteLine($"If you are on a different user profile or PC, re-register the password");
                }
                return -1;
            }

            return 0;
        }

    }
}
