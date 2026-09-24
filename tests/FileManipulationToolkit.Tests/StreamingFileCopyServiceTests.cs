using FileManipulationToolkit.Core.Domain.Enums;
using FileManipulationToolkit.Infrastructure.Copying;

namespace FileManipulationToolkit.Tests;

public class StreamingFileCopyServiceTests
{
    [Fact]
    public async Task CopyAsync_CopiesFileToDestination()
    {
        var root = Path.Combine(Path.GetTempPath(), $"file-copy-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.txt");
            var destinationPath = Path.Combine(root, "nested", "destination.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.WriteAllText(sourcePath, "hello world");

            var service = new StreamingFileCopyService();

            await service.CopyAsync(sourcePath, destinationPath);

            Assert.True(File.Exists(destinationPath));
            Assert.Equal("hello world", await File.ReadAllTextAsync(destinationPath));
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
    public async Task CopyAsync_WhenCancelled_RemovesPartialFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"file-copy-cancel-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.bin");
            var destinationPath = Path.Combine(root, "destination.bin");
            await File.WriteAllBytesAsync(sourcePath, Enumerable.Repeat((byte)7, 1024 * 1024).ToArray());

            var service = new StreamingFileCopyService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.CopyAsync(sourcePath, destinationPath, cancellationToken: cts.Token));

            Assert.False(File.Exists(destinationPath));
            Assert.False(File.Exists(destinationPath + ".partial"));
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
