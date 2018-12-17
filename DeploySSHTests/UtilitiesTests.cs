using System;

using DeploySSH.Executers;
using DeploySSH.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeploySSHTests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void VariableExpand()
        {
            var bag = new PipelineBag();
            bag.SetValue(PipelineBag.ProjectDir, "c:\\temp");
            bag.SetValue(PipelineBag.ProjectName, "myproject");
            bag.SetValue(PipelineBag.PublishDir, "c:\\bar");
            bag.SetValue(PipelineBag.AssemblyName, "myassembly");

            Assert.AreEqual("", "".Expand(bag));
            Assert.AreEqual("aa", "aa".Expand(bag));
            Assert.ThrowsException<Exception>(() => "$()".Expand(bag));
            Assert.ThrowsException<Exception>(() => " $()".Expand(bag));
            Assert.ThrowsException<Exception>(() => "$() ".Expand(bag));
            Assert.AreEqual(@"-c:\temp\raf", @"-$(projectdir)\raf".Expand(bag));
            Assert.AreEqual(@"-myproject", @"-$(projectname)".Expand(bag));
            Assert.AreEqual(@"-myassembly", @"-$(assemblyname)".Expand(bag));
            Assert.AreEqual(@"-c:\bar", @"-$(publishdir)".Expand(bag));
            Assert.ThrowsException<Exception>(() => @"-$(unknown)".Expand(bag));
            Assert.AreEqual(@"$(", @"$(".Expand(bag));
            Assert.AreEqual(@"a$(", @"a$(".Expand(bag));
            Assert.AreEqual(@"a$( ", @"a$( ".Expand(bag));
            Assert.AreEqual(@"a() ", @"a() ".Expand(bag));
        }
    }
}
