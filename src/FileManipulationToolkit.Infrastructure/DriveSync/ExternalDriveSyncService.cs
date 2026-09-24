using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.DriveSync;

public class ExternalDriveSyncService : IExternalDriveSyncService
{
    public async Task<List<string>> MirrorToDriveAsync(string sourceDirectory, string driveRoot, string mirrorFolderName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(driveRoot))
        {
            throw new ArgumentException("Drive root is required.", nameof(driveRoot));
        }

        if (string.IsNullOrWhiteSpace(mirrorFolderName))
        {
            throw new ArgumentException("Mirror folder name is required.", nameof(mirrorFolderName));
        }

        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");
        }

        var mirrorRoot = Path.Combine(driveRoot, mirrorFolderName);
        Directory.CreateDirectory(mirrorRoot);

        var copiedFiles = new List<string>();
        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var destinationFile = Path.Combine(mirrorRoot, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var safeTarget = GetAvailableDestinationPath(destinationFile);
            await CopyFileAsync(file, safeTarget, cancellationToken);
            copiedFiles.Add(safeTarget);
        }

        return copiedFiles;
    }

    public async Task<List<string>> MirrorFromDriveAsync(string driveRoot, string destinationDirectory, string mirrorFolderName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(driveRoot))
        {
            throw new ArgumentException("Drive root is required.", nameof(driveRoot));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new ArgumentException("Destination directory is required.", nameof(destinationDirectory));
        }

        if (string.IsNullOrWhiteSpace(mirrorFolderName))
        {
            throw new ArgumentException("Mirror folder name is required.", nameof(mirrorFolderName));
        }

        var mirrorRoot = Path.Combine(driveRoot, mirrorFolderName);
        if (!Directory.Exists(mirrorRoot))
        {
            throw new DirectoryNotFoundException($"Mirror folder does not exist: {mirrorRoot}");
        }

        var copiedFiles = new List<string>();
        foreach (var file in Directory.EnumerateFiles(mirrorRoot, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(mirrorRoot, file);
            var destinationFile = Path.Combine(destinationDirectory, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var safeTarget = GetAvailableDestinationPath(destinationFile);
            await CopyFileAsync(file, safeTarget, cancellationToken);
            copiedFiles.Add(safeTarget);
        }

        return copiedFiles;
    }

    private static async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken)
    {
        await using var source = File.OpenRead(sourceFile);
        await using var destination = File.Create(destinationFile);
        await source.CopyToAsync(destination, cancellationToken);
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
