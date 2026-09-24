using FileManipulationToolkit.Core.Services.Interfaces;
using FileManipulationToolkit.Infrastructure.FileSystem;
using FileManipulationToolkit.Infrastructure.Organization;

namespace FileManipulationToolkit.Tests;

public class FileOrganizationServiceTests
{
    [Fact]
    public async Task OrganizeAsync_MovesFilesIntoCategoryFolders()
    {
        var root = Path.Combine(Path.GetTempPath(), $"organizer-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "incoming");
        var targetRoot = Path.Combine(root, "organized");
        Directory.CreateDirectory(sourceRoot);

        var pdfFile = Path.Combine(sourceRoot, "report.pdf");
        var imageFile = Path.Combine(sourceRoot, "photo.jpg");
        var textFile = Path.Combine(sourceRoot, "notes.txt");

        await File.WriteAllTextAsync(pdfFile, "pdf");
        await File.WriteAllTextAsync(imageFile, "image");
        await File.WriteAllTextAsync(textFile, "text");

        var service = new FileOrganizationService(new WindowsFileSystemService());

        try
        {
            var moved = await service.OrganizeAsync(sourceRoot, targetRoot);

            Assert.Equal(3, moved.Count);
            Assert.True(File.Exists(Path.Combine(targetRoot, "Documents", "report.pdf")));
            Assert.True(File.Exists(Path.Combine(targetRoot, "Images", "photo.jpg")));
            Assert.True(File.Exists(Path.Combine(targetRoot, "Text", "notes.txt")));
            Assert.False(File.Exists(pdfFile));
            Assert.False(File.Exists(imageFile));
            Assert.False(File.Exists(textFile));
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
    public async Task DistributeAsync_SpreadsFilesAcrossTargetFolders()
    {
        var root = Path.Combine(Path.GetTempPath(), $"distributor-{Guid.NewGuid():N}");
        var dumpRoot = Path.Combine(root, "dump");
        var targetRoot = Path.Combine(root, "targets");
        Directory.CreateDirectory(dumpRoot);

        await File.WriteAllTextAsync(Path.Combine(dumpRoot, "one.txt"), "one");
        await File.WriteAllTextAsync(Path.Combine(dumpRoot, "two.txt"), "two");
        await File.WriteAllTextAsync(Path.Combine(dumpRoot, "three.txt"), "three");

        var service = new FileOrganizationService(new WindowsFileSystemService());

        try
        {
            var moved = await service.DistributeAsync(dumpRoot, targetRoot, 2);

            Assert.Equal(3, moved.Count);
            Assert.True(Directory.Exists(Path.Combine(targetRoot, "Target_1")));
            Assert.True(Directory.Exists(Path.Combine(targetRoot, "Target_2")));
            Assert.True(File.Exists(Path.Combine(targetRoot, "Target_1", "one.txt")) || File.Exists(Path.Combine(targetRoot, "Target_2", "one.txt")));
            Assert.False(Directory.Exists(dumpRoot) && Directory.EnumerateFiles(dumpRoot).Any());
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
