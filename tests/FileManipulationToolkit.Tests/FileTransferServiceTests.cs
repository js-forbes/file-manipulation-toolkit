using FileManipulationToolkit.Core.Services.Interfaces;
using FileManipulationToolkit.Infrastructure.Transfer;

namespace FileManipulationToolkit.Tests;

public class FileTransferServiceTests
{
    [Fact]
    public async Task MoveFileAsync_MovesSingleFileToFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"file-transfer-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "source");
        var destinationRoot = Path.Combine(root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);

        var sourceFile = Path.Combine(sourceRoot, "example.txt");
        await File.WriteAllTextAsync(sourceFile, "hello transfer");

        var service = new FileTransferService();

        try
        {
            var movedFile = await service.MoveFileAsync(sourceFile, destinationRoot);

            Assert.Equal(Path.Combine(destinationRoot, "example.txt"), movedFile);
            Assert.True(File.Exists(movedFile));
            Assert.False(File.Exists(sourceFile));
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
    public async Task MoveFilesAsync_MovesAllFilesPreservingRelativeStructure()
    {
        var root = Path.Combine(Path.GetTempPath(), $"file-transfer-folder-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "source");
        var destinationRoot = Path.Combine(root, "destination");
        Directory.CreateDirectory(Path.Combine(sourceRoot, "nested"));
        Directory.CreateDirectory(destinationRoot);

        var sourceFile = Path.Combine(sourceRoot, "nested", "alpha.txt");
        await File.WriteAllTextAsync(sourceFile, "alpha");

        var service = new FileTransferService();

        try
        {
            var movedFiles = await service.MoveFilesAsync(sourceRoot, destinationRoot);

            Assert.Single(movedFiles);
            Assert.True(File.Exists(Path.Combine(destinationRoot, "nested", "alpha.txt")));
            Assert.False(File.Exists(sourceFile));
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
