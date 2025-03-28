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
    /// <param name="target"></param>
    /// <param name="mode"></param>
    static void ExecuteFolder(string source, string target, RunMode mode = RunMode.Compute)
    {
        /// <summary>
        /// Inconditional add mode.
        /// </summary>
        if (mode == RunMode.Add)
        {
            Write(Green, $"Adding source folder: '"); Write(source); WriteLine(Green, "'");

            var sinfo = new DirectoryInfo(source);
            if (!sinfo.Exists)
                throw new DirectoryNotFoundException($"Source folder not found: '{source}'");

            var tinfo = new DirectoryInfo(target);
            if (!tinfo.Exists) TryCommand(() =>
            {
                CreateDirectory(target, sinfo);
                FixDates(sinfo, tinfo);
                CopyAttributes(sinfo, tinfo);
            });

            var sfiles = sinfo.GetFiles();
            foreach (var sfile in sfiles)
            {
                var destination = $"{AddTerminator(target)}{sfile.Name}";
                TryCommand(() =>
                {
                    AddOrUpdateFile(sfile, destination);

                    var temp = new FileInfo(destination);
                    FixDates(sfile, temp);
                    CopyAttributes(sfile, temp);
                });
            }

            var sdirs = sinfo.GetDirectories();
            foreach (var sdir in sdirs)
            {
                var destination = $"{AddTerminator(target)}{sdir.Name}";
                ExecuteFolder(sdir.FullName, destination, RunMode.Add);
            }

            return;
        }

        /// <summary>
        /// Inconditional delete mode.
        /// </summary>
        else if (mode == RunMode.Delete)
        {
            Write(Green, $"Deleting target folder: '"); Write(target); WriteLine(Green, "'");

            var tinfo = new DirectoryInfo(target);
            if (!tinfo.Exists)
                throw new DirectoryNotFoundException($"Target folder not found: '{source}'");

            var tfiles = tinfo.GetFiles();
            foreach (var tfile in tfiles)
            {
                TryCommand(() => DeleteFile(tfile));
            }

            var tdirs = tinfo.GetDirectories();
            foreach (var tdir in tdirs)
            {
                var destination = $"{AddTerminator(target)}{tdir.Name}";
                ExecuteFolder("", destination, RunMode.Delete);
            }

            TryCommand(() =>
            {
                DeleteFolder(tinfo);
            });

            return;
        }

        /// <summary>
        /// Default comparison mode.
        /// </summary>
        else
        {
            Write(Green, "Comparing '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
            WriteLine(Green, "'");

            var sinfo = new DirectoryInfo(source);
            if (!sinfo.Exists)
                throw new DirectoryNotFoundException($"Source folder not found: '{source}'");

            var tinfo = new DirectoryInfo(target);
            if (!tinfo.Exists)
            {
                // Inconditional add from source and return...
                ExecuteFolder(source, target, RunMode.Add);
                return;
            }

            // In Windows cases are the same...
            var comparison = StringComparison.OrdinalIgnoreCase;

            // Child files...
            var sfiles = sinfo.GetFiles();
            var tfiles = tinfo.GetFiles().ToList();

            foreach (var sfile in sfiles) // Source files...
            {
                var tfile = tfiles.FirstOrDefault(
                    x => string.Compare(sfile.Name, x.Name, comparison) == 0);

                if (tfile == null)
                {
                    // No target file, just copy the source...
                    var destination = $"{AddTerminator(target)}{sfile.Name}";
                    TryCommand(() =>
                    {
                        AddOrUpdateFile(sfile, destination);

                        var temp = new FileInfo(destination);
                        FixDates(sfile, temp);
                        CopyAttributes(sfile, temp);
                    });
                }
                else
                {
                    // Comparing both files...
                    if (!CompareFiles(sfile, tfile)) TryCommand(() =>
                    {
                        AddOrUpdateFile(sfile, tfile.FullName);

                        FixDates(sfile, tfile);
                        CopyAttributes(sfile, tfile);
                    });
                    tfiles.Remove(tfile);
                }
            }

            foreach (var tfile in tfiles) // Remaining target files...
            {
                TryCommand(() =>
                {
                    DeleteFile(tfile);
                });
            }

            // Child folders...
            var sdirs = sinfo.GetDirectories();
            var tdirs = tinfo.GetDirectories().ToList();

            foreach (var sdir in sdirs) // Source childs...
            {
                var tdir = tdirs.FirstOrDefault(
                    x => string.Compare(sdir.Name, x.Name, comparison) == 0);

                if (tdir == null)
                {
                    // No target child, inconditional add from source child...
                    var destination = $"{AddTerminator(target)}{sdir.Name}";
                    ExecuteFolder(sdir.FullName, destination, RunMode.Add);
                }
                else
                {
                    // Both exist, let's compare their contents...
                    var destination = $"{AddTerminator(target)}{tdir.Name}";
                    ExecuteFolder(sdir.FullName, destination);
                    tdirs.Remove(tdir);
                }
            }

            foreach (var tdir in tdirs) // Remaning target folders...
            {
                var destination = $"{AddTerminator(target)}{tdir.Name}";
                ExecuteFolder("", destination, RunMode.Delete);
            }


            return;
        }

        /// <summary>
        /// Tries to execute the given action at most the given amount of times, and waiting at
        /// most the given milliseconds between retries. If no success, throws the last exception.
        /// </summary>
        static void TryCommand(Action action, int max = 3, int waitms = 100)
        {
            if (max < 1) max = 1;
            if (waitms < 10) waitms = 10;

            for (int i = max; i > 0; i--)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (i <= 1) throw;
                    Thread.Sleep(waitms);
                }
            }
        }
    }
}