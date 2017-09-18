# DeployTool
This tool was created with the purpose to automate the deployment of an application to a Linux ARM machine (Raspberry PI).
The typical steps run by the tool are:
- launch "dotnet publish" to create a "Self Contained Deployment" (SCD) of the application
- open an SSH connection
- remove recursively the files in all 
- copy all the files in the given folder
- set the application executable (755) permissions on Linux

Configuration files allows to chain a sequence of 'actions' that will be executed. The current available actions are the following and more actions may be added in future versions:
- spawning a "dotnet publish" CLI command
- copy files and/or directories recursively on the remote host with the option to delete the remote option first (via SSH)
- execute a command on the remote host (via SSH)
- run an application on the remote host  (via SSH)

Configuration files also benefit from using pre-defined variables "$(variablename)" to ease the customization of the configurations.

## **Disclaimer**
### **Re-read the [license](LICENSE): the author does not assume any responsibilty or liability**

> One of the actions copy a folder and its files on the remote device. Before doing it, it optionally deletes the remote folder tree. The **cleanup** is very useful but, if misconfigured, is also extremely dangerous and **potentially destructive**, especially if the credentials are administrative.
> 
> The **DeleteRemoteFolder** (see below) should be turned on only if you specified a remote folder that is safe to delete. Be advised that all the subtree will be removed using the "rm -rf foldername" command.
> 
> Example of a destructive action:
> If the remote folder is "/" and the user is "root", the deletion will wipe all the files on all the mounted devices (including USB keys or any other remote device).


## Tool description
The tool use configuration files to describe all the necessary info (connection, remote machine name, etc.) and the list of all the desired actions.

```
DeployTool v1.0.0.10 by @raffaeler (https://github.com/raffaeler/DeployTool)

dotnet-deploy create -f <filename>.deploy
    Create a new configuration file with sample data

dotnet-deploy run -f <filename>.deploy
    Process the actions described in the configuration file

dotnet-deploy protect -encrypt text
    Encrypt the provided string that can be used in the configuration file
    The encrypted string will be valid only if used in the current user profile

dotnet-deploy protect -decrypt text
    Decrypt the encrypted string
    Decryption is valid only if data was encrypted in the same user profile

dotnet-deploy interact
    Show an interactive menu allowing to process/run the desired configuration

dotnet-deploy help
    Show this help

Configuration variables:
  $(publishdir)         The output folder used by 'dotnet publish'
                        This value is available only after the publish action
  $(projectdir)         The full qualified folder where the csproj is located
  $(projectname)        The name of the csproj file (without extension)
  $(assemblyname)       The AssemblyName as read from the csproj file

Available configurations:
  device1
  device2
```

## Commands
**dotnet-deploy create -f *filename*.deploy**<br/>
**dotnet-deploy create -file *filename*.deploy**

This command creates a sample configuration json file that can be easily edited and modified as desired, avoiding the need to know the configuration schema.

**dotnet-deploy create -f <filename>.deploy -m**<br/>
**dotnet-deploy create -f <filename>.deploy -minimal**

The "minimal" option specify to create a minimalistic json configuration file containing just the most frequently used options.

**dotnet-deploy run -f <filename>.deploy**<br/>
**dotnet-deploy run -file <filename>.deploy**

This command run all the actions specified in the configuration file
For example, it can be added to the Visual Studio post-deploy actions.

**dotnet-deploy protect -encrypt <text>**

This command is used to encrypt a secret (password) to be used in the configuration file.
The configuration file accepts both encrypted and clear text passwords but it is strongly reccomended to use either an encrypted password or, for SSH connections, a certificate.
The encrypted text use th hexadecimal convention and it is valid only on the machine and user profile where it has been created.
The text is encrypted using the DPAPI or equivalent.

**dotnet-deploy protect -decrypt text**

This command is primarily used to test the decryption algorithm.
Given an hexadecimal string produced with the -encrypt option, this command decrypt and prints on the console the clear text

**dotnet-deploy interact**

This command allows to interactively run the available configuration files.
A menu is displayed on the console, assigning a number to each  configuration file, ordered by name.
If more than 9 files are available, the menu paginates the available files that can be browsed with "PgUp" and "PgDown" keys.

**dotnet-deploy help**

This command displays the help as shown above.

*Configuration variables*

A number of variables can be used inside the configuration files to allow reusing the configuration files on multiple projects.
The configuration variables are described in the help command.
The help command also lists all the configuration files found in the current directory.

**Note**

The help and protect command can be run in any folder but all the other commands need to read a NET Core csproj file. For this reason if a valid csproj file is not run when running those commands, the tool display an error.

## Configuration files
The following configuration file was created with the command: **dotnet-deploy create -f sample**

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

The minimal configuration file was created with the command: **dotnet-deploy create -f sample -m**
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

## Root options

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

### DotnetPublishAction
***ActionName*** is the name of the action

The following are exactly the same options available on the  Microsoft "dotnet publish" command. You can refer to that command to have more info.

***OutputFolder*** is the name of the folder to be used on the remote machine. The folder must be absolute because a complete removal is done first, **which can be a destructive action**.

***RuntimeIdentifier*** is the "runtime identifier" used in the Microsoft "dotnet publish" command. For example when deploying on the Raspberry PI the runtime identifier is "linux-arm".

***TargetFramework*** is an optional string that specify which version of the framework should be used on the deploy.

***Configuration*** is the string of the name of the configuration. Typically you may want to deploy the "Release", but you can specify "Debug" or any other custom configuration available.

***VersionSuffix*** is an optional suffix to be appended to the version during the deploy.

***Manifest*** is an optional custom manifest file to be used in the deploy

***IsSelfContained*** This should be true if the runtime has not been installed on the target machine, or if you want to produce an executable file. When false, the runtime is needed and the application must be run using "dotnet app.dll" command.

***Verbosity*** specifies the verbosity of the publish operation. For example "minimal".

### SshCopyToRemoteAction

***ActionName*** is the name of the action

***LocalItems*** is an array that specifies the list of the additional files to be copied in the target folder. This is needed because the dotnet publish command copies just the essential files and not those marked as content or even those that are marked to be copied in the properties.

***RemoteFolder*** is the absolute remote path to be used during the copy,

***DeleteRemoteFolder*** specify if the folder should be deleted before copying.
**Please triple check and backup the remote content before specifying true here because, if the folder is wrong, the effect can be destructive**

***Recurse*** specify that the copy should be recursive for all the children folders and files.

### SshRunCommandAction
***ActionName*** is the name of the action

***Command*** any valid Linux command

### SshRunAppAction
***ActionName*** is the name of the action

***RemoteFolder*** is the absolute name of the remote folder where the app resides.

***RemoteApp*** is the name of the executable to run

***Arguments*** is a string with the arguments to be passes to the application.

## Installation
Currently it is better to use the DeployToolFx binaries which depends on the Framework 4.6.
The DeployTool project is compiled with Net Core and it works, but it is intended to be used as a dotnet CLI tool as soon as the extensibility will offer this opportunity (there are few gotchas at the moment preventing them to work).

1. Download the release or clone the git repository and compile it
2. Copy the executable and the dependent dlls into a folder of your choice
3. [optional but strongly suggested] Add the (2) folder to the system PATH so that it can be used from any project folder.


