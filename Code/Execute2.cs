using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
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

        // If any byte is different, they are not equivalent...
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
    /// Deletes the given target element.
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

        if (!IsEmulate) target.Delete();
    }

    /// <summary>
    /// Deletes the given target element.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="log"></param>
    static void DeleteFile(FileInfo target, bool log = true)
    {
        if (log)
        {
            Logs.Add($"Deleting file: {target.FullName}");
            Write(Blue, "Deleting file: '"); Write(target.FullName); WriteLine(Blue, "'");
        }

        if (!IsEmulate) target.Delete();
    }

    // ----------------------------------------------------

    /// <summary>
    /// Creates the given target directory, whose full path is given.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <param name="log"></param>
    static void CreateDirectory(string target, DirectoryInfo source, bool log = true)
    {
        if (log)
        {
            Logs.Add($"Creating folder: {target}");
            Write(Green, "Creating folder: '"); Write(target); WriteLine(Green, "'");
        }

        if (!IsEmulate) Directory.CreateDirectory(target);
    }

    /// <summary>
    /// Adds or updates the given source file over the given target one, whose full path is
    /// given.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="log"></param>
    static void AddOrUpdateFile(FileInfo source, string target, bool log = true)
    {
        var item = new FileInfo(target);
        var desc = item.Exists ? "Updating" : "Adding";

        if (log)
        {
            Logs.Add($"{desc} file: {source.FullName}");
            Write(Yellow, $"{desc} file: '"); Write(source.FullName); WriteLine(Yellow, "'");
        }

        if (!IsEmulate) File.Copy(source.FullName, target, true);
    }

    // ----------------------------------------------------

    /// <summary>
    /// Copies the source attributes to the target element.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    static void CopyAttributes(DirectoryInfo source, DirectoryInfo target)
    {
        var attr = source.Attributes;
        target.Attributes = attr;
    }

    /// <summary>
    /// Copies the source attributes to the target element.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    static void CopyAttributes(FileInfo source, FileInfo target)
    {
        var attr = source.Attributes;
        File.SetAttributes(target.FullName, attr);
    }

    // ----------------------------------------------------

    /// <summary>
    /// Fix the source dates so that they are coherent ones, and then replicate those dates on
    /// the target element.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    static void FixDates(DirectoryInfo source, DirectoryInfo target)
    {
        // Source dates...
        var needed = false;
        var ctime = source.CreationTime; if (ctime > LocalTime) { ctime = LocalTime; needed = true; }
        var mtime = source.LastWriteTime; if (mtime < ctime) { mtime = ctime; needed = true; }
        var atime = source.LastAccessTime; if (atime < mtime) { mtime = atime; needed = true; }

        if (needed)
        {
            Directory.SetCreationTime(source.FullName, ctime); source.CreationTime = ctime;
            Directory.SetLastWriteTime(source.FullName, mtime); source.LastWriteTime = mtime;
            Directory.SetLastAccessTime(source.FullName, atime); source.LastAccessTime = atime;
        }

        // Target dates...
        needed = false;
        ctime = target.CreationTime; if (ctime != source.CreationTime) needed = true;
        mtime = target.LastWriteTime; if (mtime != source.LastWriteTime) needed = true;
        atime = target.LastAccessTime; if (atime != source.LastAccessTime) needed = true;

        if (needed)
        {
            Directory.SetCreationTime(target.FullName, source.CreationTime);
            Directory.SetLastWriteTime(target.FullName, source.LastWriteTime);
            Directory.SetLastAccessTime(target.FullName, source.LastAccessTime);
        }
    }

    /// <summary>
    /// Fix the source dates so that they are coherent ones, and then replicate those dates on
    /// the target element.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    static void FixDates(FileInfo source, FileInfo target)
    {
        // Source dates...
        var needed = false;
        var ctime = source.CreationTime; if (ctime > LocalTime) { ctime = LocalTime; needed = true; }
        var mtime = source.LastWriteTime; if (mtime < ctime) { mtime = ctime; needed = true; }
        var atime = source.LastAccessTime; if (atime < mtime) { mtime = atime; needed = true; }

        if (needed)
        {
            File.SetCreationTime(source.FullName, ctime); source.CreationTime = ctime;
            File.SetLastWriteTime(source.FullName, mtime); source.LastWriteTime = mtime;
            File.SetLastAccessTime(source.FullName, atime); source.LastAccessTime = atime;
        }

        // Target dates...
        needed = false;
        ctime = target.CreationTime; if (ctime != source.CreationTime) needed = true;
        mtime = target.LastWriteTime; if (mtime != source.LastWriteTime) needed = true;
        atime = target.LastAccessTime; if (atime != source.LastAccessTime) needed = true;

        if (needed)
        {
            File.SetCreationTime(target.FullName, source.CreationTime);
            File.SetLastWriteTime(target.FullName, source.LastWriteTime);
            File.SetLastAccessTime(target.FullName, source.LastAccessTime);
        }
    }
}