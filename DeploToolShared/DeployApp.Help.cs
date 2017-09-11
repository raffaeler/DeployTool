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
        private int ProcessHelpCommand(HelpCommand helpCommand)
        {
            Console.WriteLine("dotnet deploy tool v1.0 by @raffaeler (https://github.com/raffaeler/DeployTool)");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy create -f <filename>.deploy");
            Console.WriteLine("    Create a new configuration file with sample data");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy run -f <filename>.deploy");
            Console.WriteLine("    Process the actions described in the configuration file");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy interact");
            Console.WriteLine("    Show an interactive menu allowing to process/run the desired configuration");
            Console.WriteLine("");
            Console.WriteLine("dotnet deploy help");
            Console.WriteLine("    Show this help");
            Console.WriteLine("");
            Console.WriteLine("Configuration variables:");
            Console.WriteLine("  $(publishdir)  \tThe output folder used by 'dotnet publish' (available only after that command)");
            Console.WriteLine("  $(projectdir)  \tThe full qualified folder where the csproj is located");
            Console.WriteLine("  $(projectname) \tThe name of the csproj file (without extension)");
            Console.WriteLine("  $(assemblyname)\tThe AssemblyName as read from the csproj file");
            Console.WriteLine("");
            return 0;
        }


    }
}
