using System;
using System.Collections.Generic;
using System.Text;

using DeploySSH.CommandLine;
using DeploySSH.Configuration;
using DeploySSH.Executers;
using DeploySSH.Helpers;

namespace DeploySSH
{
    public partial class DeployApp
    {
        private int ProcessEncryptCommand(EncryptCommand encryptCommand)
        {
            try
            {
                Console.WriteLine("Type the secret to encrypt. Echo is disabled for security reasons.");
                var secret = ConsoleManager.ReadLine(true);
                if (string.IsNullOrEmpty(secret))
                {
                    Console.WriteLine("The provided secret is empty, please retry");
                    return -1;
                }

                var encrypted = DPApiHelper.Encrypt(secret);
                Console.WriteLine($"Protect values are valid *only* for the current user profile");
                Console.WriteLine($"Write the encrypted value in the configuration");
                Console.WriteLine($"Use the decrypt command with this data to see the clear text value");
                Console.WriteLine();
                var old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Encrypted:");
                Console.ForegroundColor = old;
                Console.WriteLine($"{encrypted}");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine($"If you are on a different user profile or PC, encrypt the password again");
                return -1;
            }

            return 0;
        }

    }
}
