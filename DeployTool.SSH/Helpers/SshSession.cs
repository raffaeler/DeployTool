using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Renci.SshNet;

namespace DeployTool.Helpers
{
    public class SshSession : IDisposable
    {
        private static int _globalId;
        private ConnectionInfo _connectionInfo;
        private int _connectionCount;
        private SshProgress _progress;

        private SshClient _sshClient;
        private ScpClient _scpClient;
        private SftpClient _sftpClient;

        internal SshSession(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
            Id = Interlocked.Increment(ref _globalId);
            _connectionCount = 0;
        }

        public int Id { get; }

        internal SshClient SshClient
        {
            get
            {
                if (_sshClient == null) _sshClient = new SshClient(_connectionInfo);
                return _sshClient;
            }
        }

        internal ScpClient ScpClient
        {
            get
            {
                if (_scpClient == null) _scpClient = new ScpClient(_connectionInfo);
                return _scpClient;
            }
        }

        internal SftpClient SftpClient
        {
            get
            {
                if (_sftpClient == null) _sftpClient = new SftpClient(_connectionInfo);
                return _sftpClient;
            }
        }

        internal SshClient SshClientConnected
        {
            get
            {
                if (SshClient.IsConnected) return SshClient;
                SshClient.Connect();
                SshClient.ErrorOccurred += Client_ErrorOccurred;
                _progress?.UpdateConnected(++_connectionCount);
                return SshClient;
            }
        }

        internal ScpClient ScpClientConnected
        {
            get
            {
                if (ScpClient.IsConnected) return ScpClient;
                ScpClient.Connect();
                ScpClient.Uploading += Client_Uploading;
                ScpClient.Downloading += Client_Downloading;
                ScpClient.ErrorOccurred += Client_ErrorOccurred;
                _progress?.UpdateConnected(++_connectionCount);
                return ScpClient;
            }
        }

        internal SftpClient SftpClientConnected
        {
            get
            {
                if (SftpClient.IsConnected) return SftpClient;
                SftpClient.Connect();
                SftpClient.ErrorOccurred += Client_ErrorOccurred;
                _progress?.UpdateConnected(++_connectionCount);
                return SftpClient;
            }
        }

        internal SshProgress SshProgress => _progress;

        internal void CreateProgress(string operationContext,
            long totalTransferSize, long numberOfFiles,
            Action<SshProgress> onProgress)
        {
            _progress = new SshProgress(operationContext, totalTransferSize, numberOfFiles, onProgress);
            _progress?.UpdateConnected(_connectionCount);
        }

        public void Dispose()
        {
            if (_sshClient != null && _sshClient.IsConnected)
            {
                _sshClient.Disconnect();
                _progress?.UpdateDisconnected(--_connectionCount);
            }
            if (_sshClient != null) _sshClient.Dispose();

            if (_scpClient != null && _scpClient.IsConnected)
            {
                _scpClient.Disconnect();
                _progress?.UpdateDisconnected(--_connectionCount);
            }
            if (_scpClient != null) _scpClient.Dispose();

            if (_sftpClient != null && _sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
                _progress?.UpdateDisconnected(--_connectionCount);
            }
            if (_sftpClient != null) _sftpClient.Dispose();
        }

        public override string ToString()
        {
            bool ssh = _sshClient != null && _sshClient.IsConnected;
            bool scp = _scpClient != null && _scpClient.IsConnected;
            bool sftp = _sftpClient != null && _sftpClient.IsConnected;
            return $"Session {Id} Ssh[{ssh}] Scp[{scp}] Sftp[{sftp}]";
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            _progress.UpdateProgressErrorAborting(e.Exception);
        }

        private void Client_Downloading(object sender, Renci.SshNet.Common.ScpDownloadEventArgs e)
        {
            _progress.UpdateProgress(e.Filename, e.Size, e.Downloaded);
        }

        private void Client_Uploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs e)
        {
            _progress.UpdateProgress(e.Filename, e.Size, e.Uploaded);
        }
    }
}
