using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
/// <summary>
/// Program entry point.
/// </summary>
internal partial class Program
{
    internal static string? ThisFolderDebugMode = null;
    internal static string? ThisNameDebugMode = null;
    internal static DateTime LocalTime = DateTime.Now;

    internal static string ThisFolder { get; private set; } = default!;
    internal static string ThisName { get; private set; } = default!;
    internal static string Destination { get; private set; } = default!;
    internal static bool IsHelp { get; private set; }
    internal static bool IsFast { get; private set; }
    internal static bool IsEmulate { get; private set; }
    internal static List<string> Logs { get; } = [];

    // ----------------------------------------------------

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        ParseOptions(args);

        try
        {
            if (args.Length == 0 || IsHelp) Help();
            else
            {
                ThisFolder = CaptureThisFolder();
                ThisName = CaptureThisName();
                Destination = CaptureDestination(args[0]);

                Logs.Add($"EasyBackup started at: {DateTime.Now}, local time.");
                Logs.Add($"From source:    {ThisFolder}");
                Logs.Add($"To destination: {Destination}");
                Logs.Add($"");

                WriteLine();
                WriteLine(Green, "Source:");
                Write(Green, "\tFolder:\t"); WriteLine(ThisFolder);
                Write(Green, "\tName:\t"); WriteLine(ThisName);

                WriteLine();
                Write(Green, "Destination:\t"); WriteLine(Destination);

                WriteLine();
                ExecuteFolder(ThisFolder, Destination);
            }
        }
        catch (Exception ex)
        {
            Logs.Add("");
            Logs.Add(ex.ToDisplayString());
            Logs.Add("");

            WriteLine();
            WriteLine(Red, ex.ToDisplayString());
        }
        finally
        {
            Logs.Add("");
            Logs.Add("-- End of log file --");

            var name = $"{AddTerminator(ThisFolder)}EasyBackup.log";

            using var writer = new StreamWriter(name, false);
            foreach (var item in Logs) writer.WriteLine(item);
            writer.Close();
        }

#if DEBUG
        WriteLine(); Write(Green, "Please press [Enter] to finish... ");
        ReadLine();
#endif
    }

    // ----------------------------------------------------

    /// <summary>
    /// Captures the path from where this program is running. The returned string is guaranteed
    /// to end with a terminator.
    /// </summary>
    /// <returns></returns>
    static string CaptureThisFolder()
    {
        if (ThisFolderDebugMode == null)
        {
            var dir = AppContext.BaseDirectory;
            dir = AddTerminator(dir);
            return dir;
        }
        else return ThisFolderDebugMode;
    }

    /// <summary>
    /// Captures the actual name of this running program. May throw an exception.
    /// </summary>
    /// <returns></returns>
    static string CaptureThisName()
    {
        if (ThisNameDebugMode == null)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            if (name == null)
            {
                var str = "Cannot capture the name of this running program.";

                Logs.Add(str);
                WriteLine(Red, str);
                throw new FileNotFoundException(str);
            }

            return name!;
        }
        else return ThisNameDebugMode;
    }

    // ----------------------------------------------------

    /// <summary>
    /// Captures the full path of the destination.
    /// </summary>
    /// <returns></returns>
    static string CaptureDestination(string temp)
    {
        if (!Directory.Exists(temp))
        {
            var str = $"Root destination does not exist: '{temp}'";
            Logs.Add(str);

            Write(Red, $"Root destination does not exist: '"); Write(temp); WriteLine(Red, "'");
            throw new DirectoryNotFoundException(str);
        }

        temp = Path.GetFullPath(temp);
        return temp;
    }

    // ----------------------------------------------------

    /// <summary>
    /// Returns the given folder name having guaranteed it ends with a terminator.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    static string AddTerminator(string str)
    {
        if (!str.EndsWith('\\')) str += '\\';
        return str;
    }
}