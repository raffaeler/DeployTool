using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Configuration
{
    public class DotnetPublishAction : IAction
    {
        public DotnetPublishAction()
        {
            ActionName = this.GetType().Name;
        }

        public string GetShortActionName() => "DotnetPublish";

        /// <summary>
        /// The name of this action
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// The argument -o or --output OUTPUT_DIR
        /// Output directory in which to place the published artifacts.
        /// </summary>
        public string OutputFolder { get; set; }

        /// <summary>
        /// The argument -r or --runtime RUNTIME_IDENTIFIER
        /// Publish the project for a given runtime. This is used when creating self-contained deployment.
        /// Default is to publish a framework-dependent app.
        /// </summary>
        public string RuntimeIdentifier { get; set; }

        /// <summary>
        /// The argument -f or --framework FRAMEWORK
        /// Target framework to publish for. The target framework has to be specified in the project file.
        /// </summary>
        public string TargetFramework { get; set; }

        /// <summary>
        /// The argument -c or --configuration CONFIGURATION
        /// Debug, Release, ...
        /// Configuration to use for building the project.  Default for most projects is  "Debug".
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// --version-suffix
        /// Defines the value for the $(VersionSuffix) property in the project.
        /// </summary>
        public string VersionSuffix { get; set; }

        /// <summary>
        /// --manifest manifest.xml
        /// The path to a target manifest file that contains the list of packages to be excluded from the publish step.
        /// </summary>
        public string Manifest { get; set; }

        /// <summary>
        /// --self-contained ==> --self-contained=true
        /// --self-contained = false
        /// Publish the .NET Core runtime with your application so the runtime doesn't need
        /// to be installed on the target machine. Defaults to 'true' if a runtime identifier is specified.
        /// </summary>
        public bool IsSelfContained { get; set; }

        /// <summary>
        /// --no-restore
        /// Does not do an implicit restore when executing the command.
        /// </summary>
        public bool IsNoRestore { get; set; }

        /// <summary>
        /// -v, --verbosity
        /// Set the verbosity level of the command.
        /// Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
        /// </summary>
        public string Verbosity { get; set; }

        /// <summary>
        /// --no-dependencies
        /// Set this flag to ignore project to project references and only restore the root project.
        /// </summary>
        public bool IsNoDependencies { get; set; }

        /// <summary>
        /// --force
        /// Set this flag to force all dependencies to be resolved even if the last restore was successful.
        /// This is equivalent to deleting project.assets.json.
        /// </summary>
        public bool IsForce { get; set; }

        //public string GetConstructedOutputFolder()
        //{
        //    if (!string.IsNullOrEmpty(OutputFolder))
        //    {
        //        return OutputFolder;
        //    }


        //}

        public string GetDotnetCommand()
        {
            //FillDefaults();

            var sb = new StringBuilder();
            sb.Append("publish ");
            AddIfAvailable(sb, "-o", OutputFolder);
            AddIfAvailable(sb, "-f", TargetFramework);
            AddIfAvailable(sb, "-r", RuntimeIdentifier);
            AddIfAvailable(sb, "-c", Configuration);
            AddIfAvailable(sb, "--version-suffix", VersionSuffix);
            AddIfAvailable(sb, "--manifest", Manifest);
            AddWithValueIfAvailable(sb, "--self-contained", IsSelfContained, true);
            AddIfAvailable(sb, "--no-restore", IsNoRestore);
            AddIfAvailable(sb, "-v", Verbosity);
            AddIfAvailable(sb, "--no-dependencies", IsNoDependencies);
            AddIfAvailable(sb, "--force", IsForce);
            return sb.ToString();
        }

        //private void FillDefaults()
        //{
        //    if (string.IsNullOrEmpty(Configuration))
        //    {
        //        Configuration = "Debug";
        //    }
        //}

        private void AddIfAvailable(StringBuilder sb, string option, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append($"{option} {value} ");
            }
        }

        private void AddIfAvailable(StringBuilder sb, string option, bool value)
        {
            if (value)
            {
                sb.Append($"{option}");
            }
        }

        private void AddWithValueIfAvailable(StringBuilder sb, string option, bool value, bool always)
        {
            if (value || always)
            {
                sb.Append($"{option}={value} ");
            }
        }

    }
}
