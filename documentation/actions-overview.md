# Actions overview
An action is one of the steps that are sequentially performed by the tool.

The Actions configuration section is an array and can contain one or more of the following actions.
```
  "Actions": [
      ...
  ]
```

## DotnetPublishAction
***ActionName*** is the name of the action

The following are exactly the same options available on the  Microsoft "dotnet publish" command. You can refer to that command to have more info.

***OutputFolder*** is the name of the folder to be used on the local machine. The folder can be relative, in this case is relative to the project folder.

***RuntimeIdentifier*** is the "runtime identifier" used in the Microsoft "dotnet publish" command. For example when deploying on the Raspberry PI the runtime identifier is "linux-arm".

***TargetFramework*** is an optional string that specify which version of the framework should be used on the deploy.

***Configuration*** is the string of the name of the configuration. Typically you may want to deploy the "Release", but you can specify "Debug" or any other custom configuration available.

***VersionSuffix*** is an optional suffix to be appended to the version during the deploy.

***Manifest*** is an optional custom manifest file to be used in the deploy

***IsSelfContained*** This should be true if the runtime has not been installed on the target machine, or if you want to produce an executable file. When false, the runtime is needed and the application must be run using "dotnet app.dll" command.

***Verbosity*** specifies the verbosity of the publish operation. For example "minimal".

## SshSyncRemoteAction

***ActionName*** is the name of the action.

***LocalFolder*** is the name of the local folder (on Windows).

***RemoteFolder*** is the absolute remote path to be used during the echo.

***DeleteRemoteOrphans*** is a boolean telling whether the file that only exists on the remote (Linux) side within the specified remote folder(s) should be deleted.

***Recurse*** is a boolean telling whether the echo/sync should include only the specified folder or also the subfolders.

## SshCopyToRemoteAction

***ActionName*** is the name of the action

***LocalItems*** is an array that specifies the list of the additional files to be copied in the target folder. This is needed because the dotnet publish command copies just the essential files and not those marked as content or even those that are marked to be copied in the properties.

***RemoteFolder*** is the absolute remote path to be used during the copy. The copy is most efficient when the DeleteRemoteFolder is true.

***DeleteRemoteFolder*** specify if the folder should be deleted before copying.
**Please triple check and backup the remote content before specifying true here because, if the folder is wrong, the effect can be destructive**

***Recurse*** specify that the copy should be recursive for all the children folders and files.

## SshRunCommandAction
***ActionName*** is the name of the action

***Command*** any valid Linux command

## SshRunAppAction
***ActionName*** is the name of the action

***RemoteFolder*** is the absolute name of the remote folder where the app resides.

***RemoteApp*** is the name of the executable to run

***Arguments*** is a string with the arguments to be passes to the application.
