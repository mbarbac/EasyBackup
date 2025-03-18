using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
/// <summary>
/// Program entry point.
/// </summary>
internal partial class Program
{
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

        // Easy cases...
        if (args.Length == 0 || IsHelp)
        {
            Help();
            return;
        }

        // Capturing...
        ThisFolder = CaptureThisFolder();
        ThisName = CaptureThisName();
        Destination = CaptureDestination(args[0]);

        // Presenting start-up information...
        WriteLine();
        WriteLine(Green, "Source:");
        Write(Green, "\tFolder:\t"); WriteLine(ThisFolder);
        Write(Green, "\tName:\t"); WriteLine(ThisName);

        WriteLine();
        Write(Green, "Destination:\t"); WriteLine(Destination);

        // Executing...
        try
        {
            WriteLine();
            Execute(ThisFolder, Destination, false);
        }
        finally
        {
            var name = $"{ThisFolder}{ThisName}.log";

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
    /// Logs the message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    static void AddLog(string message, params object?[] args)
    {
        args ??= [];
        if (args.Length > 0) message = string.Format(message, args);

        Logs.Add(message);
    }

    /// <summary>
    /// Logs the message and writes it to the console with the given color.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    static void AddLog(ConsoleColor color, string message, params object?[] args)
    {
        args ??= [];
        if (args.Length > 0) message = string.Format(message, args);

        Logs.Add(message);
        WriteLine(color, message);
    }

    // ----------------------------------------------------

    /// <summary>
    /// Log the given exception.
    /// </summary>
    /// <param name="ex"></param>
    static void AddLog(Exception ex)
    {
        var temp = ex.ToDisplayString();
        Logs.Add(temp);
    }

    /// <summary>
    /// Logs the message and writes it to the console with the given color.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="ex"></param>
    static void AddLog(ConsoleColor color, Exception ex)
    {
        var temp = ex.ToDisplayString();
        Logs.Add(temp);
        Write(color, temp);
    }

    // ----------------------------------------------------

    /// <summary>
    /// Captures the path from where this program is running.
    /// </summary>
    /// <returns></returns>
    static string CaptureThisFolder()
    {
        var dir = AppContext.BaseDirectory;
        if (!dir.EndsWith('\\')) dir += '\\';
        return dir;
    }

    /// <summary>
    /// Captures the actual name of this running program.
    /// </summary>
    /// <returns></returns>
    static string CaptureThisName()
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        if (name == null)
        {
            WriteLine(Red, "Cannot capture the name of this running program.");
            Environment.Exit(1);
        }

        return name!;
    }

    /// <summary>
    /// Captures the full path of the destination.
    /// </summary>
    /// <returns></returns>
    static string CaptureDestination(string temp)
    {
        if (!Directory.Exists(temp))
        {
            Write(Red, $"Root destination does not exist: '"); Write(temp); WriteLine(Red, "'");
            Environment.Exit(1);
        }

        temp = Path.GetFullPath(temp);
        return temp;
    }
}