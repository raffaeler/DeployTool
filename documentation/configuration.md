# Configuration file
The tool only works with configuration files because there are many options that need to be specified.

A configuration file can be created by mean of the "create" command.

## The root of the configuration

**Description** is a descriptive string that is never used in the actions

**Ssh/Host** specifies the remote Linux machine

**Ssh/Username** is the username to be used in the SSH connection

**Ssh/Password** is the cleartext password to access the remote machine. Please avoid using this field and use **EncryptedPassword** instead.

**Ssh/EncryptedPassword** is the hexadecimal string returned from the command: **dotnet-deploy protect -decrypt ClearTextPassword** where ClearTextPassword is the unencrypted password.

**Ssh/PrivateKeys/PrivateKeyFile** specify the name of the private key file used to access the remote machine. Please avoid putting this file under source control.

**Ssh/PrivateKeys/PassPhrase** is the secret used to access the PrivateKeyFile.
In a future version an EncryptedPassPhrase will be added to avoid specifying the PassPhrase in clear text in the configuration.

**Ssh/ProxyHost** is the name of the proxy to be used in the communication.

**Ssh/ProxyUsername** is the username to be used in accessing the proxy

**Ssh/ProxyPassword** is the password to be used in accessing the proxy. Please avoid using this field and use **EncryptedProxyPassword** instead.

**Ssh/EncryptedProxyPassword** is the hexadecimal string returned from the command: **dotnet-deploy protect -decrypt ClearTextPassword** where ClearTextPassword is the unencrypted password.

**Actions** is an array that specify, in order, the actions to be executed with the **run** command.
A detailed explanation follows in the Actions chapter.

## Actions
[See documentation](documentation/actions-overview.md)
