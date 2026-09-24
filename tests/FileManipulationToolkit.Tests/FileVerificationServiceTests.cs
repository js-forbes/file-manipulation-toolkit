using FileManipulationToolkit.Infrastructure.Verification;

namespace FileManipulationToolkit.Tests;

public class FileVerificationServiceTests
{
    [Fact]
    public async Task VerifyAsync_WhenFilesMatch_ReturnsPassedResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"verify-pass-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.txt");
            var destinationPath = Path.Combine(root, "destination.txt");
            var content = "alpha beta gamma";

            await File.WriteAllTextAsync(sourcePath, content);
            await File.WriteAllTextAsync(destinationPath, content);

            var service = new FileVerificationService();
            var result = await service.VerifyAsync(sourcePath, destinationPath, verifyFileExists: true, compareSize: true, useSha256: true);

            Assert.True(result.Passed);
            Assert.True(result.SizeMatches);
            Assert.True(result.HashMatches);
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
    public async Task VerifyAsync_WhenSizeDiffers_ReturnsFailureResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"verify-size-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.txt");
            var destinationPath = Path.Combine(root, "destination.txt");

            await File.WriteAllTextAsync(sourcePath, "abc");
            await File.WriteAllTextAsync(destinationPath, "abcd");

            var service = new FileVerificationService();
            var result = await service.VerifyAsync(sourcePath, destinationPath, verifyFileExists: true, compareSize: true, useSha256: false);

            Assert.False(result.Passed);
            Assert.False(result.SizeMatches);
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
    public async Task VerifyAsync_WhenHashDiffers_ReturnsFailureResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"verify-hash-mismatch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.txt");
            var destinationPath = Path.Combine(root, "destination.txt");

            await File.WriteAllTextAsync(sourcePath, "abc");
            await File.WriteAllTextAsync(destinationPath, "xyz");

            var service = new FileVerificationService();
            var result = await service.VerifyAsync(sourcePath, destinationPath, verifyFileExists: true, compareSize: true, useSha256: true);

            Assert.False(result.Passed);
            Assert.True(result.SizeMatches);
            Assert.False(result.HashMatches);
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
    public async Task VerifyAsync_WhenDestinationMissing_ReturnsFailureResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"verify-missing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var sourcePath = Path.Combine(root, "source.txt");
            var destinationPath = Path.Combine(root, "missing.txt");
            await File.WriteAllTextAsync(sourcePath, "test");

            var service = new FileVerificationService();
            var result = await service.VerifyAsync(sourcePath, destinationPath, verifyFileExists: true, compareSize: true, useSha256: false);

            Assert.False(result.Passed);
            Assert.False(result.Exists);
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
