# EasyBackup
Creates a backup from the folder _where this program is located_, to the destination one given as its argument.

Due to this behavior, it is **strongly discouraged** to place this program in a global automatic path, such as ```C:\Windows``` or similar ones.


## Usage
**Syntax**: ```EasyBackup destination [-s|-strict] [-e|-emulate]```

If the destination folder does not exist, then it is created. Once it is located or created, the program starts synchronizing all files from the folder where it is located, including itself, recursively.

**Folders**:
- Source directories that are not in the corresponding destination location are replicated into the later, even empty ones.
- Destination folders with no corresponding source ones are deleted from the destination, including all its contents.
- Finally, if the same folder exist both in the source and in the destination, then the source elements are synchonized over the destination location.

**Files:**

- Source files that are not in the destination location are copied into the later.
- Destination files that are not in the source location are blindly deleted.
- Finally, the program replaces the destination file with the source one, unless all the following attributes are the same: creation and last modification dates, and sizes. Note that the ```[-s|-strict]``` argument can be used to enforce a byte to byte comparison as well.

**Other considerations:**

```[-e|-emulate]```: Runs the program in simulation mode, writing into the log file a description of the operations that otherwise would have been executed.

```EasyBackup.log```: this file is automatically created each time the program is run, and then saved in both the root source location and in the root destination one. Unless the ```[-e]``` option is used, it only contains the description of the error conditions found.