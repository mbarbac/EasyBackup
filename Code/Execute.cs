using System.Transactions;
using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
    /// <summary>
    /// Tries to execute the given command a number of times.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="max"></param>
    /// <param name="sleep"></param>
    static void DoCommand(Action action, int max = 3, int sleep = 100)
    {
        if (max < 1) max = 1;
        if (sleep < 10) sleep = 10;

        for (int i = max; i > 0; i--)
        {
            try
            {
                action();
                return;
            }
            catch
            {
                Thread.Sleep(sleep);
                if (i == 1) throw;
            }
        }
    }
    
    // ----------------------------------------------------

    /// <summary>
    /// Executes the appropriate actions considering the given source and destination folders,
    /// whose FULL paths shall be given, unless a not default run mode is specified, in which
    /// case just executes its actions.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="mode"></param>
    static void ExecuteFolder(string source, string target, RunMode mode = RunMode.Compute)
    {
        var sinfo = new DirectoryInfo(source);
        var tinfo = new DirectoryInfo(target);

        /// <summary>
        /// Inconditional add mode...
        /// </summary>
        if (mode == RunMode.Add)
        {
            Logs.Add($"Adding from: '{source}' to '{target}'");
            Write(Green, "Adding from: '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
            WriteLine(Green, "'");

            if (!sinfo.Exists) throw new DirectoryNotFoundException($"Source not found: '{source}'");
            if (!tinfo.Exists) Directory.CreateDirectory(tinfo.FullName);

            var files = sinfo.GetFiles();
            foreach (var file in files)
            {
                var destination = $"{AddTerminator(target)}{file.Name}";
                AddOrUpdateFile(file, destination);
            }

            var dirs = sinfo.GetDirectories();
            foreach (var dir in dirs)
            {
                var destination = $"{AddTerminator(target)}{dir.Name}";
                ExecuteFolder(dir.FullName, destination, RunMode.Add);
            }

            return;
        }

        /// <summary>
        /// Inconditional delete mode...
        /// </summary>
        else if (mode == RunMode.Delete)
        {
            Logs.Add($"Deleting folder: '{target}'");
            Write(Green, "Deleting folder: '"); Write(target); WriteLine(Green, "'");

            if (!tinfo.Exists) throw new DirectoryNotFoundException($"Target not found: '{tinfo}'");

            var files = sinfo.GetFiles();
            foreach (var file in files)
            {
                DoCommand(() => DeleteFile(file));
            }

            var dirs = sinfo.GetDirectories();
            foreach (var dir in dirs)
            {
                var destination = $"{AddTerminator(target)}{dir.Name}";
                ExecuteFolder("", destination, RunMode.Delete);
            }

            DeleteFolder(tinfo, false);
            return;
        }

        /// <summary>
        /// Compute actions mode...
        /// </summary>
        else
        {
            Logs.Add($"Comparing '{source}' to '{target}'");
            Write(Green, "Comparing '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
            WriteLine(Green, "'");

            if (!sinfo.Exists) throw new DirectoryNotFoundException($"Source not found: '{source}'");
            if (!tinfo.Exists)
            {
                // Inconditional add from source and return...
                ExecuteFolder(source, target, RunMode.Add);
                return;
            }

            // Files...

            // Child folders...
            var sdirs = sinfo.GetDirectories();
            var tdirs = tinfo.GetDirectories().ToList();
            var comparison = StringComparison.OrdinalIgnoreCase;

            foreach (var sdir in sdirs) // Source childs...
            {
                var tdir = tdirs.FirstOrDefault(x => string.Compare(sdir.Name, x.Name, comparison) == 0);
                if (tdir == null)
                {
                    // No target child, inconditional add from source child...
                    var destination = $"{AddTerminator(target)}{sdir.Name}";
                    ExecuteFolder(sdir.FullName, destination, RunMode.Add);
                    continue;
                }
                else DoCommand(() => tdirs.Remove(tdir));
            }

            foreach (var tdir in tdirs) // Deleting remaining target childs...
            {
                var destination = $"{AddTerminator(target)}{tdir.Name}";
                ExecuteFolder("", destination, RunMode.Delete);
            }
        }
    }

    // ----------------------------------------------------

    /// <summary>
    /// Deletes the given target folder.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="log"></param>
    static void DeleteFolder(DirectoryInfo target, bool log = true)
    {
        if (log)
        {
            Logs.Add($"Deleting folder: {target.FullName}");
            Write(Magenta, "Deleting folder: '"); Write(target.FullName); WriteLine(Green, "'");
        }

        if (!IsEmulate) DoCommand(() => target.Delete());
    }

    // ----------------------------------------------------

    /// <summary>
    /// Adds the given source file to the target folder, overwriting the destination if needed.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="log"></param>
    static void AddOrUpdateFile(FileInfo source, string target, bool log = true)
    {
        if (log)
        {
            Logs.Add($"Adding file: {source.FullName}");
            Write(Yellow, "Adding file: '"); Write(source.FullName); WriteLine(Green, "'");
        }

        target = AddTerminator(target);
        var destination = $"{target}{source.Name}";
        if (!IsEmulate) DoCommand(() => File.Copy(source.FullName, destination, true));
    }

    /// <summary>
    /// Deletes the given target file.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="log"></param>
    static void DeleteFile(FileInfo target, bool log = true)
    {
        if (log)
        {
            Logs.Add($"Deleting file: {target.FullName}");
            Write(Blue, "Deleting file: '"); Write(target.FullName); WriteLine(Green, "'");
        }

        if (!IsEmulate) DoCommand(() => target.Delete());
    }
}