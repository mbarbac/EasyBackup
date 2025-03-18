using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
    /// <summary>
    /// Calculates and executes the appropriate actions considering the given source and target
    /// folders, whose full paths are given. If the delete flag is set, then no calculations are
    /// done and the destination folder is deleted along with all its contents, recursively.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="delete"></param>
    static void Execute(string source, string target, bool delete)
    {
        var root = string.Compare(ThisFolder, source, StringComparison.OrdinalIgnoreCase);

        var sdir = new DirectoryInfo(source);
        var tdir = new DirectoryInfo(target);

        if (!sdir.Exists)
        {
            AddLog(Red, $"Source directory '{source}' doesn't exist.");
            return;
        }

        // Delete case...
        if (delete)
        {
            if (!tdir.Exists) // This is easy although suspicious, let's log...
            {
                AddLog($"Target directory '{target}' cannot be delete because it doesn't exist.");
                return;
            }

            Write(Green, "Deleting: "); WriteLine(target);

            // Files in this folder...
            var files = tdir.GetFiles();
            foreach (var file in files)
            {
                Write(Green, "Deleting: "); WriteLine(file.FullName);
                try
                {
                    // if (!IsEmulate) file.Delete();
                }
                catch (Exception ex)
                {
                    AddLog(Red, $"Cannot delete file: {file.FullName}");
                    AddLog(Red, ex.ToDisplayString());
                    Environment.Exit(1);
                }
            }

            // Sub-folders...
            var dirs = tdir.GetDirectories();
            foreach (var dir in dirs) Execute(source, tdir.FullName, true);

            // Finally this folder itself...
            try
            {
                // if (!IsEmulate) tdir.Delete();
            }
            catch (Exception ex)
            {
                AddLog(Red, $"Cannot delete directory: {tdir.FullName}");
                AddLog(Red, ex.ToDisplayString());
                Environment.Exit(1);
            }

            // And do not forget to return...
            return;
        }

        // Computing options...
        else
        {
        }
    }
}