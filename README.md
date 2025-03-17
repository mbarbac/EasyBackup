# EasyBackup
Creates a backup from the folder _where this program is located_, to the destination one given as its argument.

Due to this behavior, it is **strongly discouraged** to place this program in a global automatic path, such as ```C:\Windows``` or similar ones.


## Usage
**Syntax**: ```EasyBackup destination [-f|-fast] [-e|-emulate]```

If the destination folder does not exist, then it is created. Once it is located or created, the program starts synchronizing all files from the source folder, taken as the one where the program is located, recursively. ```EasyBackup``` takes care of copying itself even if it opened and executing.

**Folders**:
- Source folders that has no corresponding ones are copied to the destination with their contents, recursively, including empty ones.
- Destination folders with no corresponding source ones are deleted from the destination, including all its contents, recursively.
- Finally, if the same folder exist both in the source and in the destination, then the source elements are synchonized over the destination location.

**Files:**

- Source files that are not in the destination locations are copied into the corresponding ones.
- Destination files that are not in the source locations are blindly deleted.
- Finally, the program decides whether or not to copy the source file over the destination one based upon the creation and last modification dates, their respective sizes, and a final byte to byte comparison if needed. The ```[-f|-fast]``` option can be used to skip that comparison if needed.

**Other considerations:**

```[-e|-emulate]```: Runs the program in simulation mode, writing into the log file a description of the operations that otherwise would have been executed.

```EasyBackup.log```: this file is automatically created each time the program is run, and then saved in both the root source location and in the root destination one. Unless the ```[-e]``` option is used, it only contains the description of the error conditions found.