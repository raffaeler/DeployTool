using DeployTool.Executers;
using DeployTool.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeployToolTests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void VariableExpand()
        {
            var bag = new PipelineBag();
            bag.SetValue(PipelineBag.ProjectDir, "c:\\temp");

            Assert.AreEqual("", "".Expand(bag));
            Assert.AreEqual("aa", "aa".Expand(bag));
            Assert.AreEqual("", "$()".Expand(bag));
            Assert.AreEqual(" ", " $()".Expand(bag));
            Assert.AreEqual(" ", "$() ".Expand(bag));
            Assert.AreEqual(@"-c:\temp\raf", @"-$(projectdir)\raf".Expand(bag));
            Assert.AreEqual(@"-", @"-$(projectname)".Expand(bag));
            Assert.AreEqual(@"-", @"-$(publishdir)".Expand(bag));
            Assert.AreEqual(@"-", @"-$(unknown)".Expand(bag));
            Assert.AreEqual(@"$(", @"$(".Expand(bag));
            Assert.AreEqual(@"a$(", @"a$(".Expand(bag));
            Assert.AreEqual(@"a$( ", @"a$( ".Expand(bag));
            Assert.AreEqual(@"a() ", @"a() ".Expand(bag));
        }
    }
}
