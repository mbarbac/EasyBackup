using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
    /// <summary>
    /// Shows a help summary.
    /// </summary>
    static void Help()
    {
        WriteLine();
        Write(Green, """
            Creates a backup from the folder where this program is located to the destination one given as its
            first argument. Due to this behavior, please do not place this program in any global search path
            (as C:\Windows or similar ones).

            Syntax: 
            """);

        WriteLine("EasyBackup destination [-f|-fast] [-e|-emulate] [-h|-help]");

        WriteLine();
        WriteLine(Green, """
            If the destination folder does not exist, then an error is displayed. Once validated, the program
            starts synchronizing the source contents over the given destination, recursively. Any errors found
            are written to a log file whose name is the name of this program, and with '.log' as its extension.
            """);

        WriteLine();
        Write("Folders: ");
        WriteLine(Green, """
            Source folders with no corresponding destination are just copied into the later. Destination
            ones with no corresponding source are just blindly deleted, along with all their contents. Finally,
            if they exist in both locations, their own contents are synchronized recursively.
            """);

        WriteLine();
        Write("Files: ");
        Write(Green, """
            Source files with no corresponding destination ones are just copied to destination. Destination
            ones with no corresponding source are just blindly deleted. Finally, the program decides whether
            or not to overrwrite the destination by, firstly, comparing the last modification date, and then
            by comparing byte to byte their respective contents. The 
            """);
        Write("[-f|-fast] ");

        WriteLine(Green, """
            option can be used to skip this
            comparison, if needed.
            """);

        WriteLine();
        Write("[-e|-emulate]: ");
        Write(Green, """
            Just emulates the actions to perform, without actually executing them, and without writting
            any log files.
            """);

        WriteLine();
    }
}