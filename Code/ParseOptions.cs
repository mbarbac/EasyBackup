using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
internal partial class Program
{
    /// <summary>
    /// Parses the command-line options.
    /// </summary>
    /// <param name="args"></param>
    static void ParseOptions(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var value = args[i].ToLower();

            switch (args[i])
            {
                case "-s":
                    var str = args[++i];
                    ThisFolderDebugMode = Path.GetDirectoryName(str);
                    ThisNameDebugMode = Path.GetFileName(str);
                    break;

                case "-h":
                case "-help":
                    IsHelp = true;
                    break;

                case "-f":
                case "-fast":
                    IsFast = true;
                    break;

                case "-e":
                case "-emulate":
                    IsEmulate = true;
                    break;
            }
        }
    }
}