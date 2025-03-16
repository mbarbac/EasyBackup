namespace EasyBackup;

// ========================================================
/// <summary>
/// Represents a wrapper around the <see cref="Console"/> class.
/// </summary>
internal static class ConsoleEx
{
    /// <summary>
    /// Writes the specified message to the standard output stream.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void Write(string? message, params object?[] args)
    {
        if (message is null) return;
        if (message.Length == 0) return;

        args ??= [null];
        if (args.Length > 0) message = string.Format(message, args);

        Console.Write(message);
    }

    /// <summary>
    /// Writes the specified message to the standard output stream, using the specified foreground
    /// color.
    /// </summary>
    /// <param name="foreground"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void Write(ConsoleColor foreground, string? message, params object?[] args)
    {
        var old = Console.ForegroundColor; Console.ForegroundColor = foreground;
        Write(message, args);
        Console.ForegroundColor = old;
    }

    /// <summary>
    /// Writes the specified message to the standard output stream, using the specified foreground
    /// and background colors.
    /// </summary>
    /// <param name="foreground"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void Write(
        ConsoleColor foreground, ConsoleColor background, string? message, params object?[] args)
    {
        var old = Console.BackgroundColor; Console.BackgroundColor = background;
        Write(foreground, message, args);
        Console.BackgroundColor = old;
    }

    // ----------------------------------------------------

    /// <summary>
    /// Writes a new line to the standard output stream.
    /// </summary>
    internal static void WriteLine() => Console.WriteLine();

    /// <summary>
    /// Writes the specified message to the standard output stream, followed by a new line.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void WriteLine(string? message, params object?[] args)
    {
        Write(message, args);
        WriteLine();
    }

    /// <summary>
    /// Writes the specified message to the standard output stream, using the specified foreground
    /// color, followed by a new line.
    /// </summary>
    /// <param name="foreground"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void WriteLine(ConsoleColor foreground, string? message, params object?[] args)
    {
        Write(foreground, message, args);
        WriteLine();
    }

    /// <summary>
    /// Writes the specified message to the standard output stream, using the specified foreground
    /// and background colors, followed by a new line.
    /// </summary>
    /// <param name="foreground"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    internal static void WriteLine(
        ConsoleColor foreground, ConsoleColor background, string? message, params object?[] args)
    {
        Write(foreground, background, message, args);
        WriteLine();
    }

    // ----------------------------------------------------

    /// <summary>
    /// Reads the next line of characters from the standard input stream. Returns the next line
    /// of characters from the input stream, or null if no more lines are available.
    /// </summary>
    /// <returns></returns>
    internal static string? ReadLine() => Console.ReadLine();

    /// <summary>
    /// Reads the next line of characters from the standard input stream, using the given foreground
    /// color. Returns the next line of characters from the input stream, or null if no more lines
    /// are available.
    /// </summary>
    /// <param name="foreground"></param>
    /// <returns></returns>
    internal static string? ReadLine(ConsoleColor foreground)
    {
        var old = Console.ForegroundColor; Console.ForegroundColor = foreground;
        var result = ReadLine();
        Console.ForegroundColor = old;
        return result;
    }

    /// <summary>
    /// Reads the next line of characters from the standard input stream, using the given foreground
    /// and backgrounbd colors. Returns the next line of characters from the input stream, or null
    /// if no more lines are available.
    /// </summary>
    /// <param name="foreground"></param>
    /// <param name="background"></param>
    /// <returns></returns>
    internal static string? ReadLine(ConsoleColor foreground, ConsoleColor background)
    {
        var old = Console.BackgroundColor; Console.BackgroundColor = background;
        var result = ReadLine(foreground);
        Console.BackgroundColor = old;
        return result;
    }
}