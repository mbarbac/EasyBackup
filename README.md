# EasyBackup
Creates a backup from the folder _where this program is located_, to the destination one given as its argument. Due to this behavior, it is **strongly discouraged** to place this program in a global automatic path, such as ```C:\Windows``` or similar ones.


## Usage
**Syntax**: ```EasyBackup destination [-f|-fast] [-e|-emulate]```

If the destination folder does not exist, then an error is displayed and the program aborted. Once it is located or created, the program starts synchronizing all files from the source folder, taken as the one where the program is located, recursively. ```EasyBackup``` takes care of copying itself even if it opened and executing.