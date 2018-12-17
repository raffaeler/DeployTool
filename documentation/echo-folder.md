# Scenario: echo a local folder to a remote machine
This document explains how to configure `deployssh` to keep a Windows folder in sync with a remote folder over an ssh connection. Usually the remote folder is a Linux machine, but recently Windows implemented SSH too.

## Important Note
#### _The implemented sync is one way. This means that any new or removed file on the remote side will be _not_ propagated on the local side._

## Creating the configuration for the echo mode

Create a new configuration file using the following command. A new file called `abcd.deploy` will be created using the template 'echo'.
```
deployssh create -f abcd -e
```
The content of the file will be similar to the following:
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
2. Open the configuration file using any editor
-   Delete the **Ssh/Password** line. While it is possible to write clear-text passwords, it should never be done for obvious security reasons.
-   Replace the **Ssh/Host** value with the name of the Linux DNS hostname
-   Replace the **Ssh/Username** field with the name of the Linux user. Be warned to avoid using the "root" user as much as possible because that account is too privileged.
-   Replace the **Actions/LocalFolder** value with the Windows folder (absolute or relative to the current directory) to copy on the Linux side
-   Replace the **Actions/RemoteFolder** value with the Linux folder where the copy will happen.
The folder **must** start with either "/" or "~/". By default Linux only permits to superusers writing on "/" folders.
The "~" symbol is the alias for the home folder of the user used to log in. If you manually log into the Linux machine using the same user the terminal will open into the "~" folder. Typing "cd [enter]" will bring you back to the home folder.
-   The **Actions/DeleteRemoteOrphans** is meant to delete the files on the Linux remote folder that are not available on the Windows side. Remote sub-folders will not be removed. Never specify true when the remote folder already contains files other than the replicated ones, or they will be deleted.
-   Replace the **Actions/Recurse** to true. This property specifies whether the sync should include the whole tree and not only the first-level folder.

## Generate an Encrypted Password
Password encryption is made through Windows DPAPI. This means that any encryption/decryption will only work in the context of the same user profile on the same Windows machine.

If you copy a configuration file with the encrypted password on another machine, the decryption will fail because the profile is not the same.

Generating an encrypted password is straightforward:
```
deployssh encrypt
Type the secret to encrypt. Echo is disabled for security reasons.
```
You now type the password. For security reasons the console will not echo the typed characters.
The output will be something like this:
```
Protect values are valid *only* for the current user profile
Write the encrypted value in the configuration
Use the decrypt command with this data to see the clear text value

Encrypted:
01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000086491E1A684DC9A03EBF9E3A1C71BB200000000004800000A0000000100000002B90B3FEE8407D49D85DF293A209C19A100000007738864DC6DD1EC01821C117D7B2DB44140000004D04DB4261DAA7FA77B3EC3D2CA4AA3857172EE0
```
Now replace the **Ssh/EncryptedPassword** value of the above configuration with the above hexadecimal string:
```
"EncryptedPassword": "01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000086491E1A684DC9A03EBF9E3A1C71BB200000000004800000A0000000100000002B90B3FEE8407D49D85DF293A209C19A100000007738864DC6DD1EC01821C117D7B2DB44140000004D04DB4261DAA7FA77B3EC3D2CA4AA3857172EE0"
```

## Preview the results
Before running the configuration, you can see a detailed log with the operations without making any changes to the local and remote file systems.
In the preview mode, the tool uses the same configuration so that the exact paths and file names are printed in the log and explaining what are the operations that would be executed in the "interactive" mode.

Start the tool in preview mode:
```
deployssh preview
```
The console will show a menu like this:
```
[Preview / readonly mode] ....
Select a configuration file or 'q' to quit

1. abcd (Test: SshSyncRemote)
```
*Note: the dots of the first line have two possible values. The first is "No project found" is shown when the current folder does not contain a csproj file. If instead a csproj is found, the name of the project is printed. This message is just a warning and does not prevent the echo to work.*

- Pressing the key "1" will start the actions described in the configuration.
- Pressing the key "q" will quit the preview session.

Here is an example of previewing the above configuration, assuming the `somefolder` has two subfolders `foo` and `bar`. Remember that this configuration has **Actions/DeleteRemoteOrphans** set to false.
```
Sync-Echo: sha -> ~/someremotefolder
As much as 5 files will be copied to the remote side
As much as 1 folder will be copied to the remote side
The included sub-folders are:
foo, bar

Press any key to continue
```

By setting the configuration property **Actions/DeleteRemoteOrphans** to true, since the **Actions/Recurse** is also true, the output would also include the following final line. 
```
Warning: the remote folder AND subfolders are subject to deletion
```
This warning is just a reminder that the files (within the specified folders and subfolders) could be subject to deletion if they are not found in the local (Windows) folder.

If instead **Actions/Recurse** is false and **Actions/DeleteRemoteOrphans** is true, the warning is printed with a slightly different message as the subfolders are untouched.
```
Warning: the remote folder (but not subfolders) are subject to deletion
```

Since the preview mode does not open a connection to the remote side, it cannot predict which files would be deleted (if any). This feature is under evaluation and may be implemented in a future version.

### **Be careful: what you don't want to do**
Never specify **Actions/DeleteRemoteOrphans** when a remote folder contains more files than the ones you have on Windows. The files that are on only on the Linux side will be **deleted**. If the **Actions/Recurse** is true, the deletion will happen for all the subfolders.

Never specify **Actions/RemoteFolder** to point to an OS folder with root privileges. Overwriting or deleting OS files may result in data loss. While this configuration is legit, you should carefully understand what you are doing before running it.

### How to safely test the configuration
1. Create and use a new unprivileged (non-root, non-sudoers) Linux user with a default home folder
2. Specify a **Actions/RemoteFolder** such as "~/test". The folder will be created and will contain the copy of the local Windows folder.
3. If you create new files on Linux and repeat the sync, those files will be deleted.
4. If you delete some file on Linux and repeat the sync, those files will be copied back again from Windows.
5. If you modify some file on Linux and repeat the sync, those files will be overwritten.

The comparison is made using a SHA1 hash on both the Linux and Windows side to ensure the content is really different. The file size and date are not used when comparing the files.

#### **Warning: Before running the configuration you must double-check it in order to avoid any data loss. In no case the author(s) or the contributor(s) may be considered responsible for any kind of data loss.**


## Running the sync
Start the tool interactively:
```
deployssh interact
```
The console will show a menu like this:
```
Select a configuration file or 'q' to quit

1. abcd (Test: SshSyncRemote)
```
*Note: if "No project found" is shown in the starting line, it means it has not found any csproj file in the folder. This is not required and does not preven the sync to happen.*

- Pressing the key "1" will start the actions described in the configuration.
- Pressing the key "q" will quit the interactive session.


