using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployTool.CommandLine;
using DeployTool.Configuration;
using DeployTool.Executers;
using DeployTool.Helpers;

// Typical command lines:
// help                     show the help for this tool
// create -f raf -m         create a minimalistic raf.deploy configuration
// create -f raf            create a full raf.deploy configuration
// interact                 show a console menu with all the available config files
// run -f raf               runt the actions described in raf.deploy

namespace DeployTool
{
    class Program
    {
        private static DeployApp _app;

        static int Main(string[] args)
        {
            _app = new DeployApp();

            return _app.ProcessCLI(args);
        }

        private static void obsolete()
        {


            DumpProject();


            var config = new SshConfiguration();
            config.Host = "colibri-t30";
            config.Username = "root";
            config.Password = null;
            config.Port = 22;
            config.ProxyHost = null;
            config.ProxyUsername = null;
            config.ProxyPassword = null;
            config.ProxyPort = 8080;
            config.PrivateKeys = null;
            var transfer = new SshTransfer(config);

            transfer.SshCopyFileToRemote(new System.IO.FileInfo(@"h:\3d\raf.txt"), "/temp/1/2/3/aaaa/caaaaa");
            transfer.SshRunCommand("ls");
            transfer.SshRemoveRemoteFolderTree("/temp");
            //transfer.Transfer(new System.IO.DirectoryInfo(@"H:\3D"), "/temp", true, "/temp/raf.txt");

            //DirectoryWalker walker = new DirectoryWalker(new System.IO.DirectoryInfo("h:\\temp"), true, (f, r) =>
            //{
            //    Console.WriteLine($"{f.FullName}\t{r}");
            //});

            Console.WriteLine("Hello World!");
        }

        private static void DumpProject()
        {
            var command = new DotnetPublishAction();
            command.IsSelfContained = true;
            command.RuntimeIdentifier = "linux-arm";
            var exec = new DotNetPublishExecuter(command);
            exec.Execute(null);

            //var executer = new DotNetExecuter("publish", "-c Release -r linux-arm --self-contained=false");
            //executer.ExecuteAndWait();
        }


    }

}
