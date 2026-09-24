using FileManipulationToolkit.Core.Domain.Enums;
using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Infrastructure.Backup;
using FileManipulationToolkit.Infrastructure.Configuration;
using FileManipulationToolkit.Infrastructure.Copying;
using FileManipulationToolkit.Infrastructure.Deletion;
using FileManipulationToolkit.Infrastructure.FileSystem;
using FileManipulationToolkit.Infrastructure.Logging;
using FileManipulationToolkit.Infrastructure.Retention;
using FileManipulationToolkit.Infrastructure.Verification;

namespace FileManipulationToolkit.Tests;

public class RollingBackupEngineTests
{
    [Fact]
    public async Task RunJobAsync_CopyOnly_CreatesBackupSetAndPreservesRelativeStructure()
    {
        var root = Path.Combine(Path.GetTempPath(), $"backup-engine-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "source");
        var backupRoot = Path.Combine(root, "backup");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(backupRoot);

        var sourceFile = Path.Combine(sourceRoot, "Config", "settings.xml");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        await File.WriteAllTextAsync(sourceFile, "<config />");

        var config = new ApplicationConfig
        {
            GlobalSettings = new GlobalSettings
            {
                RetentionCount = 3,
                OperationMode = BackupOperationMode.CopyOnly,
                Verification = new VerificationSettings { VerifyCopiedFiles = true, UseSHA256 = true }
            },
            Jobs =
            [
                new BackupJob
                {
                    Name = "Test Job",
                    SourceFolder = sourceRoot,
                    BackupFolder = backupRoot,
                    Enabled = true,
                    UseGlobalSetting = true,
                    OperationModeOverride = BackupOperationMode.CopyOnly
                }
            ]
        };

        var configService = new JsonConfigurationService();
        configService.Save(config);

        var engine = new RollingBackupEngine(
            configService,
            new WindowsFileSystemService(),
            new StreamingFileCopyService(),
            new FileVerificationService(),
            new FileDeletionService(),
            new RetentionService(),
            new FileLogger());

        try
        {
            var result = await engine.RunJobAsync(config.Jobs[0], CancellationToken.None);

            Assert.Equal(BackupStatus.Successful, result.Status);
            Assert.Equal(1, result.SuccessfulFiles);

            var createdRoot = Directory.EnumerateDirectories(backupRoot).Single();
            Assert.StartsWith("Backup_", Path.GetFileName(createdRoot));
            Assert.True(File.Exists(Path.Combine(createdRoot, "Config", "settings.xml")));
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
    public async Task RunJobAsync_WhenSourceDoesNotExist_ReturnsFailedResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"backup-engine-missing-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "missing-source");
        var backupRoot = Path.Combine(root, "backup");
        Directory.CreateDirectory(backupRoot);

        var config = new ApplicationConfig
        {
            Jobs =
            [
                new BackupJob
                {
                    Name = "Missing Source",
                    SourceFolder = sourceRoot,
                    BackupFolder = backupRoot,
                    Enabled = true,
                    UseGlobalSetting = true,
                    OperationModeOverride = BackupOperationMode.CopyOnly
                }
            ]
        };

        var configService = new JsonConfigurationService();
        configService.Save(config);

        var engine = new RollingBackupEngine(
            configService,
            new WindowsFileSystemService(),
            new StreamingFileCopyService(),
            new FileVerificationService(),
            new FileDeletionService(),
            new RetentionService(),
            new FileLogger());

        try
        {
            var result = await engine.RunJobAsync(config.Jobs[0], CancellationToken.None);

            Assert.Equal(BackupStatus.Failed, result.Status);
            Assert.Contains("does not exist", result.Errors.First());
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
