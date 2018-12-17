using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeploySSH.CommandLine;
using DeploySSH.Configuration;
using DeploySSH.Executers;
using DeploySSH.Helpers;
namespace DeploySSH
{
    public partial class DeployApp
    {
        private int ProcessHelpCommand(HelpCommand helpCommand)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"DeploySSH v{version} by @raffaeler (https://github.com/raffaeler/DeployTool)");
            Console.WriteLine($"");
            Console.WriteLine($"deployssh create -f <filename>.deploy [-m|-e]");
            Console.WriteLine($"    Create a new configuration file with sample data");
            Console.WriteLine($"    -m -minimal Minimalistic configuration sample");
            Console.WriteLine($"    -e -echo Echo only configuration sample");
            Console.WriteLine($"");
            Console.WriteLine($"deployssh run -f <filename>.deploy");
            Console.WriteLine($"    Process the actions described in the configuration file");
            Console.WriteLine($"");

            Console.WriteLine($"deployssh encrypt");
            Console.WriteLine($"    Prompt and encrypt with DPAPI a string. Paste the result in the configuration file");
            Console.WriteLine($"    The encrypted string can be only used in the context of the current user profile");
            Console.WriteLine($"");
            Console.WriteLine($"deployssh decrypt");
            Console.WriteLine($"    Prompt and decrypt an encrypted string.");
            Console.WriteLine($"    Decryption is valid only if data was encrypted in the same user profile");
            Console.WriteLine($"");

            //Console.WriteLine($"deployssh protect -encrypt text");
            //Console.WriteLine($"    Encrypt the provided string that can be used in the configuration file");
            //Console.WriteLine($"    The encrypted string will be valid only if used in the current user profile");
            //Console.WriteLine($"");
            //Console.WriteLine($"deployssh protect -decrypt text");
            //Console.WriteLine($"    Decrypt the encrypted string");
            //Console.WriteLine($"    Decryption is valid only if data was encrypted in the same user profile");
            //Console.WriteLine($"");
            Console.WriteLine($"deployssh interact");
            Console.WriteLine($"    Show an interactive menu allowing to process/run the desired configuration");
            Console.WriteLine($"");
            Console.WriteLine($"deployssh preview");
            Console.WriteLine($"    Same behavior of 'interactive' but in readonly mode");
            Console.WriteLine($"    Print a detail log of the operations that would be run using 'interactive'");
            Console.WriteLine($"");
            Console.WriteLine($"deployssh help");
            Console.WriteLine($"    Show this help");
            Console.WriteLine($"");
            //Console.WriteLine($"Configuration variables:");
            //Console.WriteLine($"  $(publishdir)  \tThe output folder used by 'dotnet publish'");
            //Console.WriteLine($"                 \tThis value is available only after the publish action");
            //Console.WriteLine($"  $(projectdir)  \tThe full qualified folder where the csproj is located");
            //Console.WriteLine($"  $(projectname) \tThe name of the csproj file (without extension)");
            //Console.WriteLine($"  $(assemblyname)\tThe AssemblyName as read from the csproj file");
            //Console.WriteLine($"");

            Console.WriteLine($"Configuration variables (values are visible when run in a project folder):");
            Console.WriteLine($"  $(publishdir)  \tThe output folder used by 'dotnet publish'");
            Console.WriteLine($"                 \tAvailable only after the publish action");
            Console.WriteLine($"  $(projectdir)  \t{GetProjectDir()}");
            Console.WriteLine($"  $(projectname) \t{GetProjectName()}");
            Console.WriteLine($"  $(assemblyname)\t{GetAssemblyName()}");

            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            var configs = di.GetFiles($"*.{Constants.DeployExtension}", System.IO.SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name);
            if (configs.Any())
            {
                Console.WriteLine($"Available configurations:");
                foreach (var fi in configs)
                {
                    Console.WriteLine("  " + System.IO.Path.GetFileNameWithoutExtension(fi.Name));
                }
            }

            Console.WriteLine();
            return 0;
        }

        private string GetProjectDir()
        {
            var value = _executerManager.Bag.Expand("$(projectdir)", false);
            if (!string.IsNullOrEmpty(value)) return value;

            return "The full qualified folder where the csproj is located";
        }

        private string GetProjectName()
        {
            var value = _executerManager.Bag.Expand("$(projectname)", false);
            if (!string.IsNullOrEmpty(value)) return value;

            return "The name of the csproj file (without extension)";
        }

        private string GetAssemblyName()
        {
            var value = _executerManager.Bag.Expand("$(assemblyname)", false);
            if (!string.IsNullOrEmpty(value)) return value;

            return "The AssemblyName as read from the csproj file";
        }
    }
}
