using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DeployTool.Helpers
{
    public class ProjectHelper
    {
        private string _projectFilename;

        public ProjectHelper()
        {
            var currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            var projects = currentFolder.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
            if (projects.Length == 0)
            {
                throw new Exception($"The current folder {currentFolder} does not contain any csproj file");
            }

            if (projects.Length > 1)
            {
                throw new Exception($"The current folder {currentFolder} contains more than one csproj file");
            }

            _projectFilename = projects.Single().FullName;
            ProjectName = Path.GetFileNameWithoutExtension(_projectFilename);

            LoadXml();
        }

        public string AssemblyName { get; private set; }
        public string ProjectName { get; private set; }

        private void LoadXml()
        {
            var xml = XElement.Load(_projectFilename);
            var assemblyNameNodes = xml.Descendants("AssemblyName");
            var assemblyNameNode = assemblyNameNodes.FirstOrDefault();
            if (assemblyNameNode == null)
            {
                throw new Exception($"The project file {_projectFilename} does not contain the \"AssemblyName\" node");
            }

            AssemblyName = assemblyNameNode.Value;
        }
    }
}
