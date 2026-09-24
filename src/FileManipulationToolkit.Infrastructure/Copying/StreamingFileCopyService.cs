using System.Diagnostics;
using FileManipulationToolkit.Core.Domain.Enums;
using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Copying;

public class StreamingFileCopyService : IFileCopyService
{
    private const int DefaultBufferSize = 1024 * 1024;

    public async Task CopyAsync(
        string sourcePath,
        string destinationPath,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("Destination path is required.", nameof(destinationPath));
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source file was not found: {sourcePath}", sourcePath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var sourceInfo = new FileInfo(sourcePath);
        var parentDirectory = Path.GetDirectoryName(destinationPath);

        if (!string.IsNullOrEmpty(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        var tempDestination = destinationPath + ".partial";
        if (File.Exists(tempDestination))
        {
            File.Delete(tempDestination);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize, FileOptions.SequentialScan);
            using var destinationStream = new FileStream(tempDestination, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, FileOptions.SequentialScan);

            var buffer = new byte[DefaultBufferSize];
            long totalBytes = sourceInfo.Length;
            long bytesCopied = 0;
            var lastReport = DateTime.UtcNow;
            var lastBytes = 0L;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var read = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                {
                    break;
                }

                await destinationStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                bytesCopied += read;

                if (progress is not null && DateTime.UtcNow - lastReport >= TimeSpan.FromMilliseconds(250))
                {
                    var elapsed = stopwatch.Elapsed;
                    var bytesSinceLast = bytesCopied - lastBytes;
                    var speed = elapsed.TotalSeconds > 0 ? bytesSinceLast / elapsed.TotalSeconds : 0;
                    var remainingBytes = totalBytes - bytesCopied;

                    progress.Report(new BackupProgress
                    {
                        JobName = Path.GetFileName(sourcePath),
                        Phase = BackupPhase.Copying,
                        CurrentFile = sourcePath,
                        CurrentFileSize = totalBytes,
                        TotalFiles = 1,
                        BytesCompleted = bytesCopied,
                        TotalBytes = totalBytes,
                        SpeedBytesPerSecond = speed,
                        Elapsed = elapsed,
                        EstimatedRemaining = speed > 0 ? TimeSpan.FromSeconds(remainingBytes / speed) : TimeSpan.Zero
                    });

                    lastReport = DateTime.UtcNow;
                    lastBytes = bytesCopied;
                }
            }

            await destinationStream.FlushAsync(cancellationToken);
            destinationStream.Dispose();

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(tempDestination, destinationPath);
        }
        catch
        {
            if (File.Exists(tempDestination))
            {
                try
                {
                    File.Delete(tempDestination);
                }
                catch
                {
                }
            }

            throw;
        }
    }
}
