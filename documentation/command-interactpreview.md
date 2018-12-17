# Interact command

**deployssh interact**

This command allows to interactively run the available configuration files.
A menu is displayed on the console, assigning a number to each  configuration file, ordered by name.
If more than 9 files are available, the menu paginates the available files that can be browsed with "PgUp" and "PgDown" keys.


**deployssh preview**

This command shows the same menu of the interact command but prints a verbose output on the actions that would be run interactively.
This is a good way to verify the configuration as it only works in read-only mode.