# Create command
The create command provides a number of templates with typical configurations.

**deployssh create -f *filename*.deploy**<br/>
**deployssh create -file *filename*.deploy**

This command creates a sample configuration json file that can be easily edited and modified as desired, avoiding the need to know the configuration schema.

**deployssh create -f <filename>.deploy -m**<br/>
**deployssh create -f <filename>.deploy -minimal**

The "minimal" option specify to create a minimalistic json configuration file containing just the most frequently used options.

**deployssh create -f <filename>.deploy -e**<br/>
**deployssh create -f <filename>.deploy -echo**

The "echo" option specify to create a json configuration file to sync (echo) a folder on the remote side.


## Echo configuration
The simplest configuration is the echo configuration: **deployssh create -f sample -e**. The echo mode is [fully explained here](echo-folder.md).
```
{
  "Description": "Test",
  "Ssh": {
    "Host": "machine-name",
    "Username": "username",
    "Password": "password",
    "EncryptedPassword": "password"
  },
  "Actions": [
    {
      "ActionName": "SshSyncRemoteAction",
      "LocalFolder": "somefolder",
      "RemoteFolder": "someremotefolder",
      "DeleteRemoteOrphans": false,
      "Recurse": false
    }
  ]
```

## Verbose configuration
The following configuration file was created with the command: **deployssh create -f sample**

```
{
  "Description": "Test",
  "Ssh": {
    "Host": "machine-name",
    "Username": "username",
    "Password": "password",
    "EncryptedPassword": "password",
    "PrivateKeys": [
      {
        "PrivateKeyFile": "privatekeyfile.key",
        "PassPhrase": "passphrase"
      }
    ],
    "ProxyHost": "proxy-host",
    "ProxyUsername": "proxy-username",
    "ProxyPassword": "proxy-password",
    "EncryptedProxyPassword": "proxy-password"
  },
  "Actions": [
    {
      "ActionName": "DotnetPublishAction",
      "OutputFolder": "publish-linux-arm",
      "RuntimeIdentifier": "linux-arm",
      "TargetFramework": "netcoreapp2.0",
      "Configuration": "Release",
      "VersionSuffix": "suffix",
      "Manifest": "manifest.xml",
      "IsSelfContained": true,
      "Verbosity": "minimal"
    },
    {
      "ActionName": "SshCopyToRemoteAction",
      "LocalItems": [
        "$(publishdir)",
        "sqlite.db"
      ],
      "RemoteFolder": "/$(projectname)",
      "DeleteRemoteFolder": false,
      "Recurse": true
    },
    {
      "ActionName": "SshRunCommandAction",
      "Command": "ls"
    },
    {
      "ActionName": "SshRunAppAction",
      "RemoteFolder": "/$(projectname)",
      "RemoteApp": "$(assemblyname)",
      "Arguments": "hello"
    }
  ]
}
```

## Minimal configuration
The minimal configuration file was created with the command: **deployssh create -f sample -m**
```
{
  "Description": "Test",
  "Ssh": {
    "Host": "machine-name",
    "Username": "username",
    "Password": "password",
    "EncryptedPassword": "password"
  },
  "Actions": [
    {
      "ActionName": "DotnetPublishAction",
      "RuntimeIdentifier": "linux-arm",
      "Configuration": "Release",
      "IsSelfContained": true
    },
    {
      "ActionName": "SshCopyToRemoteAction",
      "LocalItems": [
        "$(publishdir)"
      ],
      "RemoteFolder": "/$(projectname)",
      "DeleteRemoteFolder": false,
      "Recurse": true
    }
  ]
}
```
