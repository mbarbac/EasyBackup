using static EasyBackup.ConsoleEx;
using static System.ConsoleColor;

namespace EasyBackup;

// ========================================================
/// <summary>
/// Determines the execution mode.
/// </summary>
internal enum RunMode
{
    Compute,    // Compute the actual mode, per each element in the source folder.
    Add,        // Adds the source elements to the destination.
    Delete,     // Deletes the destination elements.
}