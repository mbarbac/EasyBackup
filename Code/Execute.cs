using System.IO.MemoryMappedFiles;
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
        /// <summary>
        /// Inconditional add mode...
        /// </summary>
        if (mode == RunMode.Add)
        {
            var sinfo = new DirectoryInfo(source);
            var tinfo = new DirectoryInfo(target);

            //Logs.Add($"Adding from: '{source}' to '{target}'");
            //Write(Green, "Adding from: '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
            //WriteLine(Green, "'");

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
            var tinfo = new DirectoryInfo(target);

            //Logs.Add($"Deleting folder: '{target}'");
            //Write(Magenta, "Deleting folder: '"); Write(target); WriteLine(Green, "'");

            if (!tinfo.Exists) throw new DirectoryNotFoundException($"Target not found: '{tinfo}'");

            var files = tinfo.GetFiles();
            foreach (var file in files)
            {
                DoCommand(() => DeleteFile(file));
            }

            var dirs = tinfo.GetDirectories();
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
            var sinfo = new DirectoryInfo(source);
            var tinfo = new DirectoryInfo(target);

            //Logs.Add($"Comparing '{source}' to '{target}'");
            //Write(Green, "Comparing '"); Write(source); Write(Green, "' to: '"); Write($"{target}");
            //WriteLine(Green, "'");

            if (!sinfo.Exists) throw new DirectoryNotFoundException($"Source not found: '{source}'");
            if (!tinfo.Exists)
            {
                // Inconditional add from source and return...
                ExecuteFolder(source, target, RunMode.Add);
                return;
            }

            var comparison = StringComparison.OrdinalIgnoreCase;

            // Child files...
            var sfiles = sinfo.GetFiles();
            var tfiles = tinfo.GetFiles().ToList();

            foreach (var sfile in sfiles) // Source files...
            {
                var tfile = tfiles.FirstOrDefault(x => string.Compare(sfile.Name, x.Name, comparison) == 0);
                if (tfile == null)
                {
                    // No target file, just copy the source...
                    var destination = $"{AddTerminator(target)}{sfile.Name}";
                    AddOrUpdateFile(sfile, destination);
                }
                else
                {
                    // Comparing both files...
                    if (!CompareFiles(sfile, tfile)) AddOrUpdateFile(sfile, tfile.FullName);
                    tfiles.Remove(tfile);
                }
            }

            foreach (var tfile in tfiles) // Deleting remaining target files...
            {
                DeleteFile(tfile);
            }

            // Child folders...
            var sdirs = sinfo.GetDirectories();
            var tdirs = tinfo.GetDirectories().ToList();

            foreach (var sdir in sdirs) // Source childs...
            {
                var tdir = tdirs.FirstOrDefault(x => string.Compare(sdir.Name, x.Name, comparison) == 0);
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
            Write(Magenta, "Deleting folder: '"); Write(target.FullName); WriteLine(Magenta, "'");
        }

        if (!IsEmulate) DoCommand(() => target.Delete());
    }

    // ----------------------------------------------------

    // Two common buffer of 1 MB each...
    const int BufferSize = 1024 * 1024;
    static byte[] SourceBuffer = new byte[BufferSize];
    static byte[] TargetBuffer = new byte[BufferSize];

    /// <summary>
    /// Invoked to determine if the two given files shall be considered equal or not.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    static bool CompareFiles(FileInfo source, FileInfo target)
    {
        // If source last write is recent, let's copy source over target...
        var slastmod = source.LastWriteTime;
        var tlastmod = target.LastWriteTime;
        if (slastmod > tlastmod) return false;

        // If different size, they are obviously not equivalent...
        if (source.Length != target.Length) return false;

        // If any byte is different, lthey are not equivalent...
        Array.Clear(SourceBuffer, 0, BufferSize);
        Array.Clear(TargetBuffer, 0, BufferSize);
        int nsource;
        int ntarget;

        using var sstream = source.OpenRead();
        using var tstream = target.OpenRead();

        while (
            (nsource = sstream.Read(SourceBuffer, 0, BufferSize)) > 0 &&
            (ntarget = tstream.Read(TargetBuffer, 0, BufferSize)) > 0)
        {
            if (nsource != ntarget || !SourceBuffer.SequenceEqual(TargetBuffer)) return false;
        }

        // Finally, we'll assume both are equivalent (we don't check internal streams)...
        return true;
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
        var file = new FileInfo(target);
        var desc = file.Exists ? "Updating" : "Adding";

        if (log)
        {
            Logs.Add($"{desc} file: {source.FullName}");
            Write(Yellow, $"{desc} file: '"); Write(source.FullName); WriteLine(Yellow, "'");
        }

        //if (!IsEmulate) DoCommand(() => File.Copy(source.FullName, target, true));

        if (!IsEmulate) DoCommand(() =>
        {
            File.Copy(source.FullName, target, true);

            using var str = file.Open(FileMode.Open, FileAccess.ReadWrite);
            using var handle = str.SafeFileHandle;

            var date = source.CreationTime; File.SetCreationTime(handle, date);
            date = source.LastAccessTime; File.SetLastAccessTime(handle, date);
            date = source.LastWriteTime; File.SetLastWriteTime(handle, date);
        });
    }
    /*
     public static class FileInfoExtensions
{
    public static IntPtr GetFileHandle(this FileInfo fileInfo, FileAccess fileAccess = FileAccess.Read)
    {
        var fileStream = fileInfo.Open(fileAccess);
        return fileStream.SafeFileHandle.DangerousGetHandle();
    }
}
     */

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