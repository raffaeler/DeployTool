## Configuration variables
The configuration values may specify variables that will be replaced at runtime with the values relevant to the project and folder in use.

The configuration variables are the following:
- $(publishdir) The output folder used by 'dotnet publish'. This value is available only after the publish action
- $(projectdir) The full qualified folder where the csproj is located
- $(projectname) The name of the csproj file (without extension)
- $(assemblyname) The AssemblyName as read from the csproj file

When running the tool within a project folder, the final values of some variables are printed in the help output.
```
deployssh

...

Configuration variables (values are visible when run in a project folder):
  $(publishdir)         The output folder used by 'dotnet publish'
                        Available only after the publish action
  $(projectdir)         c:\projects\pong
  $(projectname)        pong
  $(assemblyname)       pong
```
