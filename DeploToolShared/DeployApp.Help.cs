using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;
namespace DeployTool
{
    internal partial class DeployApp
    {
        private int ProcessHelpCommand(HelpCommand helpCommand)
        {
#if DOTNETCLI
            string sep = " ";
#else
            string sep = "-";
#endif
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"DeployTool v{version} by @raffaeler (https://github.com/raffaeler/DeployTool)");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy create -f <filename>.deploy");
            Console.WriteLine($"    Create a new configuration file with sample data");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy run -f <filename>.deploy");
            Console.WriteLine($"    Process the actions described in the configuration file");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy protect -encrypt text");
            Console.WriteLine($"    Encrypt the provided string that can be used in the configuration file");
            Console.WriteLine($"    The encrypted string will be valid only if used in the current user profile");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy protect -decrypt text");
            Console.WriteLine($"    Decrypt the encrypted string");
            Console.WriteLine($"    Decryption is valid only if data was encrypted in the same user profile");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy interact");
            Console.WriteLine($"    Show an interactive menu allowing to process/run the desired configuration");
            Console.WriteLine($"");
            Console.WriteLine($"dotnet{sep}deploy help");
            Console.WriteLine($"    Show this help");
            Console.WriteLine($"");
            Console.WriteLine($"Configuration variables:");
            Console.WriteLine($"  $(publishdir)  \tThe output folder used by 'dotnet publish'");
            Console.WriteLine($"                 \tThis value is available only after the publish action");
            Console.WriteLine($"  $(projectdir)  \tThe full qualified folder where the csproj is located");
            Console.WriteLine($"  $(projectname) \tThe name of the csproj file (without extension)");
            Console.WriteLine($"  $(assemblyname)\tThe AssemblyName as read from the csproj file");
            Console.WriteLine($"");
            Console.WriteLine($"Available configurations:");
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            var configs = di.GetFiles($"*.{Constants.DeployExtension}", System.IO.SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name);
            foreach (var fi in configs)
            {
                Console.WriteLine("  " + System.IO.Path.GetFileNameWithoutExtension(fi.Name));
            }

            Console.WriteLine();
            return 0;
        }


    }
}
