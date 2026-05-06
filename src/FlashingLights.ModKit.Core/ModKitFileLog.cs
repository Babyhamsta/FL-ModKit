using System.Globalization;

namespace FlashingLights.ModKit.Core;

public sealed class ModKitFileLog : IDisposable
{
    private static readonly TimeSpan FailureCooldown = TimeSpan.FromSeconds(30);
    private const int MaxConsecutiveFailures = 3;

    private readonly object gate = new();
    private readonly Action<string> info;
    private readonly Action<string> warn;
    private int consecutiveFailures;
    private DateTimeOffset cooldownUntil = DateTimeOffset.MinValue;
    private bool headerWritten;
    private bool disposed;

    public ModKitFileLog(string modId, Action<string> info, Action<string> warn, string? baseDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            throw new ArgumentException("Mod id cannot be empty.", nameof(modId));
        }

        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(warn);

        this.info = info;
        this.warn = warn;
        FilePath = ModKitPaths.LogPath($"{modId}.log", baseDirectory);
        ModKitPaths.EnsureParentDirectory(FilePath);
    }

    public string FilePath { get; }

    public bool LogToConsole { get; set; } = true;

    public bool LogToFile { get; set; } = true;

    public void Header(string text)
    {
        if (disposed || headerWritten)
        {
            return;
        }

        if (LogToFile)
        {
            TryWrite(text + Environment.NewLine, append: true);
        }
        headerWritten = true;
    }

    public void Info(string message)
    {
        if (disposed)
        {
            return;
        }

        if (LogToConsole)
        {
            info(message);
        }

        if (LogToFile)
        {
            TryWrite($"{Timestamp()} {message}{Environment.NewLine}", append: true);
        }
    }

    public void Warn(string message)
    {
        if (disposed)
        {
            return;
        }

        warn(message);

        if (LogToFile)
        {
            TryWrite($"{Timestamp()} WARN {message}{Environment.NewLine}", append: true);
        }
    }

    public void Debug(string message)
    {
        if (disposed || !LogToFile)
        {
            return;
        }

        TryWrite($"{Timestamp()} DEBUG {message}{Environment.NewLine}", append: true);
    }

    public void Flush()
    {
        // File writes are synchronous; this is a no-op kept for forward-compat
        // if we ever switch to a buffered writer.
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        Flush();
        disposed = true;
    }

    private static string Timestamp() =>
        DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

    private void TryWrite(string text, bool append)
    {
        if (DateTimeOffset.UtcNow < cooldownUntil)
        {
            return;
        }

        try
        {
            lock (gate)
            {
                if (append)
                {
                    File.AppendAllText(FilePath, text);
                }
                else
                {
                    File.WriteAllText(FilePath, text);
                }
            }
            consecutiveFailures = 0;
        }
        catch (IOException ex)
        {
            HandleWriteFailure(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            HandleWriteFailure(ex);
        }
    }

    private void HandleWriteFailure(Exception ex)
    {
        consecutiveFailures++;
        if (consecutiveFailures >= MaxConsecutiveFailures)
        {
            cooldownUntil = DateTimeOffset.UtcNow.Add(FailureCooldown);
            consecutiveFailures = 0;
            warn($"File logging paused {FailureCooldown.TotalSeconds:0}s after repeated failure for '{FilePath}': {ex.Message}");
        }
    }
}
