using FileManipulationToolkit.Core.Services.Interfaces;
using FileManipulationToolkit.Infrastructure.DriveSync;

namespace FileManipulationToolkit.Tests;

public class ExternalDriveSyncServiceTests
{
    [Fact]
    public async Task MirrorToDriveAsync_CopiesFilesIntoDriveMirrorFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"drive-mirror-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "source");
        var driveRoot = Path.Combine(root, "drive");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(driveRoot);

        var sourceFile = Path.Combine(sourceRoot, "nested", "report.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        await File.WriteAllTextAsync(sourceFile, "mirror me");

        var service = new ExternalDriveSyncService();

        try
        {
            var copied = await service.MirrorToDriveAsync(sourceRoot, driveRoot, "Backups");

            Assert.Single(copied);
            Assert.True(File.Exists(Path.Combine(driveRoot, "Backups", "nested", "report.txt")));
            Assert.True(File.Exists(sourceFile));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public async Task MirrorFromDriveAsync_CopiesFilesFromDriveToLocalFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"drive-sync-{Guid.NewGuid():N}");
        var driveRoot = Path.Combine(root, "drive");
        var destinationRoot = Path.Combine(root, "destination");
        Directory.CreateDirectory(driveRoot);

        var sourceFile = Path.Combine(driveRoot, "Backups", "archive", "notes.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        await File.WriteAllTextAsync(sourceFile, "hello from drive");

        var service = new ExternalDriveSyncService();

        try
        {
            var copied = await service.MirrorFromDriveAsync(driveRoot, destinationRoot, "Backups");

            Assert.Single(copied);
            Assert.True(File.Exists(Path.Combine(destinationRoot, "archive", "notes.txt")));
            Assert.True(File.Exists(sourceFile));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
