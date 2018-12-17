using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeploySSH.Configuration;
using DeploySSH.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeploySSHTests
{
    [TestClass]
    public class SSHTests
    {
        [TestMethod]
        public void Connect()
        {
            var manager = GetTestManager();

            using (var session = manager.CreateSession())
            {
                Assert.IsFalse(session.ScpClient.IsConnected);
                Assert.IsFalse(session.SftpClient.IsConnected);
                Assert.IsFalse(session.SshClient.IsConnected);

                Assert.IsTrue(session.ScpClientConnected.IsConnected);
                Assert.IsTrue(session.SftpClientConnected.IsConnected);
                Assert.IsTrue(session.SshClientConnected.IsConnected);
            }
        }


        [TestMethod]
        public void CreateSingleLevelRemoteFolder()
        {
            var manager = GetTestManager();

            using (var session = manager.CreateSession())
            {
                //session.SshCreateRemoteFolder("/sshtest-absolute");
                //session.SshRemoveRemoteFolderTree("/sshtest-absolute");

                Assert.IsTrue(session.SshCreateRemoteFolder("~/sshtest-home"));
                Assert.IsFalse(session.SshCreateRemoteFolder("~/sshtest-home"));
                Assert.IsTrue(session.SshRemoveRemoteFolderTree("~/sshtest-home"));
                Assert.IsFalse(session.SshRemoveRemoteFolderTree("~/sshtest-home"));
            }
        }


        [TestMethod]
        public void CreateRemoteFolder()
        {
            var manager = GetTestManager();

            using (var session = manager.CreateSession())
            {
                session.SshCreateRemoteFolder("~/sshtest-X/1/2/3");
                session.SshRemoveRemoteFolderTree("~/sshtest-X");
            }
        }

        [TestMethod]
        public void SyncFolder_Lib()
        {
            var manager = GetTestManager();

            using (var session = manager.CreateSession())
            {
                var stamps = session.GetSha1ForTree("certs");
            }
        }

        [TestMethod]
        public void EchoFolder()
        {
            var manager = GetTestManager();

            using (var session = manager.CreateSession())
            {
                // create a local structure on disk
                var root = CreateTestTree();
                // sync on the remote file where no file still exists
                var result1 = session.EchoFoldersRecursive(root, "~/test-delete");
                Assert.IsTrue(result1.Success);
                Assert.AreEqual(result1.Skipped, 0);
                Assert.AreEqual(result1.CopiedOver, 0);
                Assert.AreEqual(result1.CopiedNew, 9);

                // now, we make some changes on the remote side
                // two missing files
                Assert.IsTrue(session.SshDeleteRemoteFile("~/test-delete/fc.txt"));
                Assert.IsFalse(session.SshDeleteRemoteFile("~/test-delete/fc.txt"));
                session.SshDeleteRemoteFile("~/test-delete/sub1/f2sub1.txt");
                // one file with changed (trigger the wrong hash)
                session.SshRemoteCopy("~/test-delete/sub2/sub21/f1sub21.txt", "~/test-delete/sub2/sub21/f2sub21.txt");
                // one new file
                session.Touch("~/test-delete/abcdef");

                // now we sync again, expecting only the strictly needed changes, driven by the hashes
                var result2 = session.EchoFoldersRecursive(root, "~/test-delete");
                Assert.IsTrue(result2.Success);
                Assert.AreEqual(result2.Skipped, 6);
                Assert.AreEqual(result2.CopiedOver, 1);
                Assert.AreEqual(result2.CopiedNew, 2);
                Assert.AreEqual(result2.Removed, 1);

                // remove the test files locally and remotely
                DeleteTestTree(root);
                session.SshRemoveRemoteFolderTree("~/test-delete");
            }
        }

        private SshManager GetTestManager()
        {
            var config = new SshConfiguration()
            {
                // connect to the local "Linux on Windows" subsystem
                // with sshd configured appropriately
                Host = "tadev",

                // this is the Linux username
                Username = "raf",

                // this is the password encrypted using the Windows DPAPI. This string is returned by the DeployTool:
                // dotnet-deploy protect -encrypt cleartextpassword
                EncryptedPassword = "01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000015E1BA079EED9A73BAB0334FDFECBB480000000004800000A0000000100000004B554D8D85B9EFDFD8AD1BF3A014D81B100000005BE16A9FDDB4C6ADCC5735244A9FB2CE14000000D40FE74BD12FB6F9C5610E42D742938D1A782C4C",
            };

            return new SshManager(config);
        }

        private DirectoryInfo CreateTestTree()
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            var root = current.CreateSubdirectory("test-delete");
            var sub1 = root.CreateSubdirectory("sub1");
            var sub11 = sub1.CreateSubdirectory("sub11");
            var sub2 = root.CreateSubdirectory("sub2");
            var sub21 = sub2.CreateSubdirectory("sub21");

            CreateContentFile(Path.Combine(root.FullName, "fc.txt"));
            CreateContentFile(Path.Combine(sub1.FullName, "f1sub1.txt"));
            CreateContentFile(Path.Combine(sub1.FullName, "f2sub1.txt"));
            CreateContentFile(Path.Combine(sub11.FullName, "f1sub11.txt"));
            CreateContentFile(Path.Combine(sub11.FullName, "f2sub11.txt"));
            CreateContentFile(Path.Combine(sub2.FullName, "f1sub2.txt"));
            CreateContentFile(Path.Combine(sub2.FullName, "f2sub2.txt"));
            CreateContentFile(Path.Combine(sub21.FullName, "f1sub21.txt"));
            CreateContentFile(Path.Combine(sub21.FullName, "f2sub21.txt"));

            return root;
        }

        private void CreateContentFile(string filename) => File.WriteAllText(filename, filename);

        private void DeleteTestTree(DirectoryInfo directoryInfo)
        {
            // safety
            if (!directoryInfo.Name.Contains("test"))
                return;
            directoryInfo.Delete(true);
        }
    }
}
