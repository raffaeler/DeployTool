//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using DeployTool.Configuration;
//using DeployTool.Helpers;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DeployToolTests
//{
//    [TestClass]
//    public class SSHTests_old
//    {
//        [TestMethod]
//        public void ConnectSfp()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            transfer.ConnectScp(c =>
//            {
//                Assert.IsTrue(c.IsConnected);
//            });
//        }

//        [TestMethod]
//        public void ConnectSftp()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            transfer.ConnectSftp(c =>
//            {
//                Assert.IsTrue(c.IsConnected);
//            });
//        }

//        [TestMethod]
//        public void CreateRemoteFolder_Lib()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            transfer.ConnectSftp(c =>
//            {
//                // not allowed
//                //c.CreateDirectory("~/t1/t2/t3");
//                //c.CreateDirectory("r1/r2/r3");

//                //c.CreateDirectory("~/t1");
//                //c.CreateDirectory("r1");
//                //Assert.IsTrue(c.IsConnected);
//            });
//        }

//        [TestMethod]
//        public void CreateSingleLevelRemoteFolder()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            //transfer.SshCreateRemoteFolder("/sshtest-absolute");
//            //transfer.SshRemoveRemoteFolderTree("/sshtest-absolute");

//            transfer.SshCreateRemoteFolder("~/sshtest-home");
//            transfer.SshRemoveRemoteFolderTree("~/sshtest-home");
//        }


//        [TestMethod]
//        public void CreateRemoteFolder()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            transfer.SshCreateRemoteFolder("~/sshtest-X/1/2/3");
//            transfer.SshRemoveRemoteFolderTree("~/sshtest-X");
//        }

//        [TestMethod]
//        public void SyncFolder_Lib()
//        {
//            var config = GetTestConfig();

//            var transfer = new SshTransfer(config);
//            transfer.ConnectSftp(c =>
//            {
//                CreateTestTree();
//                var stamps = transfer.GetSha1ForTree("certs");
//                DeleteTestTree();
//                //c.SynchronizeDirectories()
//                Assert.IsTrue(c.IsConnected);
//            });
//        }


//        private SshConfiguration GetTestConfig()
//        {
//            var config = new SshConfiguration()
//            {
//                // connect to the local "Linux on Windows" subsystem
//                // with sshd configured appropriately
//                Host = "tadev",

//                // this is the Linux username
//                Username = "raf",

//                // this is the password encrypted using the Windows DPAPI. This string is returned by the DeployTool:
//                // dotnet-deploy protect -encrypt cleartextpassword
//                EncryptedPassword = "01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000015E1BA079EED9A73BAB0334FDFECBB480000000004800000A0000000100000004B554D8D85B9EFDFD8AD1BF3A014D81B100000005BE16A9FDDB4C6ADCC5735244A9FB2CE14000000D40FE74BD12FB6F9C5610E42D742938D1A782C4C",
//            };

//            return config;
//        }

//        private void CreateTestTree()
//        {
//            var content = "abcdef";
//            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
//            var root = current.CreateSubdirectory("test-delete");
//            var sub1 = root.CreateSubdirectory("sub1");
//            var sub11 = sub1.CreateSubdirectory("sub11");
//            var sub2 = root.CreateSubdirectory("sub2");
//            var sub21 = sub2.CreateSubdirectory("sub21");

//            File.WriteAllText(Path.Combine(root.FullName, "fc.txt"), content);
//            File.WriteAllText(Path.Combine(sub1.FullName, "f1sub1.txt"), content);
//            File.WriteAllText(Path.Combine(sub1.FullName, "f2sub1.txt"), content);
//            File.WriteAllText(Path.Combine(sub11.FullName, "f1sub11.txt"), content);
//            File.WriteAllText(Path.Combine(sub11.FullName, "f2sub11.txt"), content);
//            File.WriteAllText(Path.Combine(sub2.FullName, "f1sub2.txt"), content);
//            File.WriteAllText(Path.Combine(sub2.FullName, "f2sub2.txt"), content);
//            File.WriteAllText(Path.Combine(sub21.FullName, "f1sub21.txt"), content);
//            File.WriteAllText(Path.Combine(sub21.FullName, "f2sub21.txt"), content);
//        }

//        private void DeleteTestTree()
//        {
//            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
//            var root = current.GetDirectories("test-delete").Single();
//            root.Delete(true);
//        }
//    }
//}
