using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DeploySSH.Helpers
{
    public class ProjectHelper
    {
        private string _projectFilename;

        public ProjectHelper()
        {
            DirectoryInfo currentFolder;
            if (Debugger.IsAttached)
            {
                currentFolder = new DirectoryInfo(
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\")));
            }
            else
            {
                currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            var projects = currentFolder.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
            if (projects.Length == 0)
            {
                throw new Exception($"No csproj found in {currentFolder}");
            }

            if (projects.Length > 1)
            {
                throw new Exception($"The folder {currentFolder} contains {projects.Length} csproj files");
            }

            _projectFilename = projects.Single().FullName;
            ProjectName = Path.GetFileNameWithoutExtension(_projectFilename);

            LoadXml();
            ProjectDir = currentFolder.FullName;
        }

        public string AssemblyName { get; private set; }
        public string ProjectName { get; private set; }
        public string ProjectDir { get; private set; }

        private void LoadXml()
        {
            var xml = XElement.Load(_projectFilename);
            var assemblyNameNodes = xml.Descendants("AssemblyName");
            var assemblyNameNode = assemblyNameNodes.FirstOrDefault();
            if (assemblyNameNode == null)
            {
                AssemblyName = ProjectName;
                return;
            }

            AssemblyName = assemblyNameNode.Value;
        }
    }
}
