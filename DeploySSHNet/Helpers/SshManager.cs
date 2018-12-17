using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeploySSH.Configuration;
using Renci.SshNet;

namespace DeploySSH.Helpers
{
    public class SshManager
    {
        private ConnectionInfo _connectionInfo;

        public SshManager(SshConfiguration configuration)
        {
            AuthenticationMethod authenticationMethod;

            PrivateKeyFile[] privateKeys = null;
            if (configuration.PrivateKeys != null)
            {
                privateKeys = configuration.PrivateKeys
                    .Select(p => Load(p.PrivateKeyFile, p.PassPhrase))
                    .Where(p => p != null)
                    .ToArray();
            }

            if (privateKeys != null && privateKeys.Length > 0)
            {
                authenticationMethod =
                    new PrivateKeyAuthenticationMethod(configuration.Username, privateKeys);
            }
            else
            {
                var password = GetPassword(configuration);
                if (!string.IsNullOrEmpty(password))
                {
                    authenticationMethod =
                            new PasswordAuthenticationMethod(configuration.Username, password);
                }
                else
                {
                    authenticationMethod =
                        new NoneAuthenticationMethod(configuration.Username);
                }
            }

            if (string.IsNullOrEmpty(configuration.ProxyHost))
            {
                _connectionInfo = new ConnectionInfo(
                    configuration.Host,
                    configuration.Port,
                    configuration.Username,
                    ProxyTypes.None,
                    null,
                    0,
                    null,
                    null,
                    authenticationMethod);
            }
            else
            {
                var proxyPassword = GetProxyPassword(configuration);

                _connectionInfo = new ConnectionInfo(
                    configuration.Host,
                    configuration.Port,
                    configuration.Username,
                    ProxyTypes.Http,
                    configuration.ProxyHost,
                    configuration.ProxyPort,
                    configuration.ProxyUsername,
                    proxyPassword,
                    authenticationMethod);
            }
        }

        public SshSession CreateSession()
        {
            return new SshSession(_connectionInfo);
        }

        private PrivateKeyFile Load(string privateKeyFile, string passphrase)
        {
            try
            {
                var pvk = new PrivateKeyFile(privateKeyFile, passphrase);
                return pvk;
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error loading PrivateKeyFile: {err.ToString()}");
                return null;
            }
        }

        private string GetPassword(SshConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.EncryptedPassword))
            {
                try
                {
                    var password = DPApiHelper.Decrypt(configuration.EncryptedPassword);
                    return password;
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Encrypted password is not valid, fallback to password: {err.Message}");
                }
            }

            return configuration.Password;
        }

        private string GetProxyPassword(SshConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.EncryptedProxyPassword))
            {
                try
                {
                    var proxyPassword = DPApiHelper.Decrypt(configuration.EncryptedProxyPassword);
                    return proxyPassword;
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Encrypted proxy password is not valid, fallback to proxy password: {err.Message}");
                }
            }

            return configuration.ProxyPassword;
        }
    }

}
