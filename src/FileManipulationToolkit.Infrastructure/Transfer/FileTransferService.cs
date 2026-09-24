using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Transfer;

public class FileTransferService : IFileTransferService
{
    public async Task<string> MoveFileAsync(string sourceFile, string destinationDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
        {
            throw new ArgumentException("Source file is required.", nameof(sourceFile));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new ArgumentException("Destination directory is required.", nameof(destinationDirectory));
        }

        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException($"Source file does not exist: {sourceFile}", sourceFile);
        }

        Directory.CreateDirectory(destinationDirectory);

        var destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
        var safeTarget = GetAvailableDestinationPath(destinationFile);

        cancellationToken.ThrowIfCancellationRequested();
        await Task.Run(() => File.Move(sourceFile, safeTarget), cancellationToken);

        return safeTarget;
    }

    public async Task<List<string>> MoveFilesAsync(string sourceDirectory, string destinationDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new ArgumentException("Destination directory is required.", nameof(destinationDirectory));
        }

        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");
        }

        Directory.CreateDirectory(destinationDirectory);

        var movedFiles = new List<string>();
        var files = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var destinationFile = Path.Combine(destinationDirectory, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var safeTarget = GetAvailableDestinationPath(destinationFile);
            await Task.Run(() => File.Move(file, safeTarget), cancellationToken);
            movedFiles.Add(safeTarget);
        }

        return movedFiles;
    }

    private static string GetAvailableDestinationPath(string destinationPath)
    {
        var candidate = destinationPath;
        var counter = 1;

        while (File.Exists(candidate))
        {
            var directory = Path.GetDirectoryName(candidate) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(candidate);
            var extension = Path.GetExtension(candidate);
            candidate = Path.Combine(directory, $"{fileName}_{counter}{extension}");
            counter++;
        }

        return candidate;
    }
}
