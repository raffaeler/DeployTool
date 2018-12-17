# DeploySSH
This tool was created with the purpose to automate the deployment of an application to a Linux machine (tested on Debian, CentOS and Raspberry PI).


The typical examples that can be achieved by the tool:
### Example 1
- launch "dotnet publish" to create a "Self Contained Deployment" (SCD) of the application
- open an SSH connection
- sync/echo the local folder to the remote Linux folder. This just copy the essential files that has been modified from the last time and deletes the files not available on the local folder. Comparison is made through **SHA1** hashes.
- set the application executable (755) permissions on Linux

### Example 2
- launch "dotnet publish" to create a "Self Contained Deployment" (SCD) of the application
- open an SSH connection
- remove recursively the files in the remote folder
- copy all the files in the given folder
- set the application executable (755) permissions on Linux

## Rationale
Configuration files allows to chain a sequence of 'actions' that will be executed. The current available actions are the following and more actions may be added in future versions:
- spawning a "dotnet publish" CLI command
- copy files and/or directories recursively on the remote host with the option to delete the remote option first (via SSH)
- execute a command on the remote host (via SSH)
- run an application on the remote host  (via SSH)

Configuration files also benefit from using pre-defined variables "$(variablename)" to ease the customization of the configurations.

## **Disclaimer**
### **Re-read the [license](LICENSE): the author does not assume any responsibilty or liability**

> Some of the `actions` can delete or replace file(s) or even an entire **folder tree** on the remote side. If misconfigured, the configuration can be dangerous and **potentially destructive**, especially if the user is a `sudoer` (admin / superuser).
> 
> The **DeleteRemoteFolder** (see below) should be turned on only if you specified a remote folder that is safe to delete. Be advised that all the subtree will be removed using the **"rm -rf foldername"** command which is destructive.
>
> The **SshSyncRemoteAction** can also delete the files in the specified remote folder sub-tree. [Read the documentation for more details](documentation/echo-folder.md).
> 
> **Example of a very destructive action:**
> If the remote folder is "/" and the user is "root", the deletion will wipe all the files on all the mounted devices (including USB keys or any other remote device).


## Tool description
The tool use configuration files to describe all the necessary info (connection, remote machine name, etc.) and the list of all the desired actions.

**deployssh help** or **deployssh**  displays the help for the tool.
```
DeploySSH v1.0.0.0 by @raffaeler (https://github.com/raffaeler/DeployTool)

deployssh create -f <filename>.deploy [-m|-e]
    Create a new configuration file with sample data
    -m -minimal Minimalistic configuration sample
    -e -echo Echo only configuration sample

deployssh run -f <filename>.deploy
    Process the actions described in the configuration file

deployssh encrypt
    Prompt and encrypt with DPAPI a string. Paste the result in the configuration file
    The encrypted string can be only used in the context of the current user profile

deployssh decrypt
    Prompt and decrypt an encrypted string.
    Decryption is valid only if data was encrypted in the same user profile

deployssh interact
    Show an interactive menu allowing to process/run the desired configuration

deployssh preview
    Same behavior of 'interactive' but in readonly mode
    Print a detail log of the operations that would be run using 'interactive'

deployssh help
    Show this help

Configuration variables (values are visible when run in a project folder):
  $(publishdir)         The output folder used by 'dotnet publish'
                        Available only after the publish action
  $(projectdir)         The full qualified folder where the csproj is located
  $(projectname)        The name of the csproj file (without extension)
  $(assemblyname)       The AssemblyName as read from the csproj file
Available configurations:
  device1
  device2
```
## Configuration variables
A number of variables can be used inside the configuration files to allow reusing the configuration files on multiple projects.
The variables are [documented here](documentation/variables.md).


## Commands
- [create](documentation/command-create.md)
- [run](documentation/command-run.md)
- [interact and preview](documentation/command-interactpreview.md)
- help (see above)

## Configuration files
[See documentation](documentation/configuration.md)

## Installation
The tool will be soon published on NuGet to be available as [Global Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

If you want to install from the sources, proceed as follows:
1. Download the release or clone the git repository and compile it
2. Open a developer command prompt and navigate to the deployssh project folder
3. Run the `nu.cmd` to create a nuget pacakge locally and install the global tool from there
4. Run the `un.cmd` to uninstall the global tool.
You can iterate through this last two points to make changes and test the results.

Any feedback, wish and issue are welcome. **Please submit an issue before requesting a pull request**.
