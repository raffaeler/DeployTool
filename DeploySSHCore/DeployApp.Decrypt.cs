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
        private int ProcessDecryptCommand(DecryptCommand DecryptCommand)
        {
            try
            {
                Console.WriteLine("Type or paste the encrypted value. Be warned the clear text secret will be printed.");
                Console.WriteLine($"Decrypting is possible only if done using the user profile where they have been encrypted");
                var input = ConsoleManager.ReadLine(false);

                var decrypted = DPApiHelper.Decrypt(input);
                Console.WriteLine();
                var old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Decrypted:");
                Console.ForegroundColor = old;
                Console.WriteLine($"{decrypted}");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine($"This encrypted value is only valid on the PC and profile where it has been encrypted.");
                return -1;
            }

            return 0;
        }
    }
}
