using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DeployTool.Executers
{
    internal class DotNetExecuter : ExecuterBase
    {
        private string _dotnetFilename = "dotnet";

        protected DotNetExecuter()
        {
        }

        public override void Execute(PipelineBag bag)
        {
            if (!bag.TryGet<string>("arguments", out string arguments))
            {
                return;
            }

            var result = ExecuteAndWait(arguments, out bool isError);
            bag.SetResult(isError, result);
        }

        protected string ExecuteAndWait(string arguments, out bool isError)
        {
            try
            {
                var processStartInfo = BuildOptions(arguments);
                var process = Process.Start(processStartInfo);
                string output = process.StandardOutput.ReadToEnd();
                var err = process.StandardError.ReadToEnd();
                //process.WaitForExit();

                process.Dispose();

                if (!string.IsNullOrEmpty(err))
                {
                    isError = true;
                    return err;
                }

                isError = false;
                return output;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
                isError = true;
                return err.Message;
            }
        }

        //private void ExecuteAndContinue(string arguments)
        //{
        //    if (_isExecuting)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        var processStartInfo = BuildOptions(arguments);
        //        _process = Process.Start(processStartInfo);
        //        _process.Exited += process_Exited;
        //    }
        //    catch (Exception err)
        //    {
        //        Debug.WriteLine(err.ToString());
        //    }
        //}

        //private void process_Exited(object sender, EventArgs e)
        //{
        //    _isExecuting = false;
        //    if (_process == null)
        //        return;

        //    _process.Dispose();
        //}

        private ProcessStartInfo BuildOptions(string arguments)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = _dotnetFilename;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = Directory.GetCurrentDirectory();

            return psi;
        }
    }
}
