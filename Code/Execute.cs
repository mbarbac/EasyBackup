using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
    /// <summary>
    /// Executes the appropriate actions considering the given source and destination folders,
    /// whose FULL paths shall be given, unless a not default run mode is specified, in which
    /// case just executes its actions.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="mode"></param>
    static void Execute(string source, string destination, RunMode mode = RunMode.Compute)
    {
        switch (mode)
        {
            case RunMode.Add: ExecuteAdd(source, destination); return;
            case RunMode.Delete: ExecuteDelete(destination); return;
        }

        WriteLine("Execute: Pending...");
    }

    // ----------------------------------------------------

    /// <summary>
    /// Inconditionally add the contents of the source directory to the destination one, whose
    /// FULL paths shall be given.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    static void ExecuteAdd(string source, string target)
    {
        var sourceinfo = new DirectoryInfo(source);
        if (!sourceinfo.Exists) throw new DirectoryNotFoundException($"Source not found: '{target}'");

        var targetinfo = new DirectoryInfo(target);
        if (!targetinfo.Exists) Directory.CreateDirectory(targetinfo.FullName);

        if (!source.EndsWith('\\')) source = source + "\\";
        if (!target.EndsWith('\\')) target = target + "\\";

        Logs.Add($"Adding from: '{source}' to '{target}'");
        Write(Green, "Adding from: '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
        WriteLine(Green, "'");

        // Source files...
        var files = sourceinfo.GetFiles();
        foreach (var file in files)
        {
            Logs.Add($"Adding file: '{file.FullName}'");
            Write(Green, "Adding file: '"); Write(file.FullName); WriteLine(Green, "'");

            var destination = $"{target}{file.Name}";
            if (!IsEmulate) File.Copy(file.FullName, destination, true);
        }

        // Child folders...
        var dirs = sourceinfo.GetDirectories();
        foreach (var dir in dirs)
        {
            var xsource = source + dir.Name;
            var xtarget = target + dir.Name;
            ExecuteAdd(xsource, xtarget);
        }
    }

    // ----------------------------------------------------

    /// <summary>
    /// Inconditionally deletes the destination folder, whose FULL path shall be given, and all
    /// its contents.
    /// </summary>
    /// <param name="target"></param>
    static void ExecuteDelete(string target)
    {
        var info = new DirectoryInfo(target);
        if (!info.Exists) throw new DirectoryNotFoundException($"Destination not found: {target}");

        Logs.Add($"Deleting folder: {target}");
        Write(Magenta, "Deleting folder: "); WriteLine(target);

        // Files in the given folder...
        var files = info.GetFiles();
        foreach (var file in files)
        {
            Logs.Add($"Deleting file:   {file.FullName}");
            Write(Blue, "Deleting file:   "); WriteLine(file.FullName);

            if (!IsEmulate) file.Delete();
        }

        // Child folders...
        var dirs = info.GetDirectories();
        foreach (var dir in dirs) Execute(string.Empty, dir.FullName, RunMode.Delete);

        // This folder...
        if (!IsEmulate) info.Delete();
    }
}