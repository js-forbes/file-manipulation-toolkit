using System.Text.RegularExpressions;
using FileManipulationToolkit.Core.Domain.Enums;
using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Backup;

public class RollingBackupEngine
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IFileCopyService _fileCopyService;
    private readonly IFileVerificationService _fileVerificationService;
    private readonly IFileDeletionService _fileDeletionService;
    private readonly IRetentionService _retentionService;
    private readonly ILoggingService _loggingService;

    private static readonly Regex BackupSetRegex = new(@"^Backup_\d{4}-\d{2}-\d{2}_\d{6}_\d{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public RollingBackupEngine(
        IConfigurationService configurationService,
        IFileSystemService fileSystemService,
        IFileCopyService fileCopyService,
        IFileVerificationService fileVerificationService,
        IFileDeletionService fileDeletionService,
        IRetentionService retentionService,
        ILoggingService loggingService)
    {
        _configurationService = configurationService;
        _fileSystemService = fileSystemService;
        _fileCopyService = fileCopyService;
        _fileVerificationService = fileVerificationService;
        _fileDeletionService = fileDeletionService;
        _retentionService = retentionService;
        _loggingService = loggingService;
    }

    public async Task<BackupResult> RunJobAsync(BackupJob job, CancellationToken cancellationToken = default)
    {
        var result = new BackupResult
        {
            Status = BackupStatus.Successful,
            Message = $"Backup job '{job.Name}' completed successfully."
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!job.Enabled)
            {
                result.Status = BackupStatus.Failed;
                result.Errors.Add("Job is disabled and was skipped.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(job.SourceFolder) || string.IsNullOrWhiteSpace(job.BackupFolder))
            {
                result.Status = BackupStatus.Failed;
                result.Errors.Add("Source and destination folders are required.");
                return result;
            }

            var globalConfig = _configurationService.Load();
            var mode = job.UseGlobalSetting
                ? globalConfig.GlobalSettings.OperationMode
                : (job.OperationModeOverride ?? globalConfig.GlobalSettings.OperationMode);

            var retentionCount = job.UseGlobalSetting
                ? globalConfig.GlobalSettings.RetentionCount
                : (job.RetentionCountOverride ?? globalConfig.GlobalSettings.RetentionCount);

            var sourceFullPath = _fileSystemService.GetFullPath(job.SourceFolder);
            var destinationFullPath = _fileSystemService.GetFullPath(job.BackupFolder);

            if (!_fileSystemService.DirectoryExists(sourceFullPath))
            {
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceFullPath}");
            }

            if (_fileSystemService.PathsAreEquivalent(sourceFullPath, destinationFullPath))
            {
                throw new InvalidOperationException("Destination folder cannot be the same as the source folder.");
            }

            if (_fileSystemService.IsPathInsideDirectory(destinationFullPath, sourceFullPath))
            {
                throw new InvalidOperationException("Destination folder cannot be inside the source folder.");
            }

            var backupSetName = CreateBackupSetName();
            var backupSetPath = Path.Combine(destinationFullPath, backupSetName);
            _fileSystemService.CreateDirectory(backupSetPath);

            var files = _fileSystemService.EnumerateFiles(sourceFullPath, recursive: true).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            result.TotalFiles = files.Count;
            result.TotalBytes = files.Sum(f => _fileSystemService.GetLength(f));

            foreach (var sourceFile in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sourceMetadata = new FileMetadata
                {
                    Length = _fileSystemService.GetLength(sourceFile),
                    LastWriteTimeUtc = _fileSystemService.GetLastWriteTimeUtc(sourceFile)
                };

                var relativePath = Path.GetRelativePath(sourceFullPath, sourceFile);
                var destinationFile = Path.Combine(backupSetPath, relativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationFile);

                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    _fileSystemService.CreateDirectory(destinationDirectory);
                }

                await _fileCopyService.CopyAsync(sourceFile, destinationFile, cancellationToken: cancellationToken);

                var verification = await _fileVerificationService.VerifyAsync(
                    sourceFile,
                    destinationFile,
                    verifyFileExists: true,
                    compareSize: true,
                    useSha256: true,
                    cancellationToken,
                    progress: null);

                if (!verification.Passed)
                {
                    result.FailedFiles++;
                    result.Errors.Add($"Verification failed for '{sourceFile}': {verification.FailureReason}");
                    continue;
                }

                if (mode == BackupOperationMode.CopyAndDelete)
                {
                    var currentSourceMetadata = new FileMetadata
                    {
                        Length = _fileSystemService.GetLength(sourceFile),
                        LastWriteTimeUtc = _fileSystemService.GetLastWriteTimeUtc(sourceFile)
                    };

                    if (sourceMetadata.Length != currentSourceMetadata.Length || sourceMetadata.LastWriteTimeUtc != currentSourceMetadata.LastWriteTimeUtc)
                    {
                        result.Errors.Add($"Source changed during backup; deletion skipped for '{sourceFile}'.");
                        continue;
                    }

                    var deleted = await _fileDeletionService.DeleteFileAsync(sourceFile, cancellationToken);
                    if (deleted)
                    {
                        result.DeletedFiles++;
                        _loggingService.Information($"DELETE SUCCESS | {sourceFile}");
                    }
                    else
                    {
                        result.DeleteFailedFiles++;
                        result.Errors.Add($"Deletion failed for '{sourceFile}'.");
                        _loggingService.Warning($"DELETE FAILED | {sourceFile}");
                    }
                }

                result.SuccessfulFiles++;
                result.ProcessedBytes += _fileSystemService.GetLength(sourceFile);
            }

            if (mode == BackupOperationMode.CopyAndDelete)
            {
                await _retentionService.ApplyRetentionAsync(destinationFullPath, retentionCount, cancellationToken);
            }

            if (result.FailedFiles > 0 && result.SuccessfulFiles > 0)
            {
                result.Status = BackupStatus.CompletedWithWarnings;
                result.Message = $"Backup job '{job.Name}' completed with warnings.";
            }
            else if (result.FailedFiles > 0 || result.DeleteFailedFiles > 0)
            {
                result.Status = BackupStatus.CompletedWithWarnings;
                result.Message = $"Backup job '{job.Name}' completed with warnings.";
            }
            else if (result.Status == BackupStatus.Successful && result.SuccessfulFiles == 0 && result.TotalFiles == 0)
            {
                result.Status = BackupStatus.Successful;
                result.Message = $"Backup job '{job.Name}' completed with no files to process.";
            }

            _loggingService.Information($"BACKUP FINISHED | {job.Name} | Status={result.Status}");
            return result;
        }
        catch (OperationCanceledException ex)
        {
            result.Status = BackupStatus.Cancelled;
            result.Errors.Add("Backup operation was cancelled.");
            _loggingService.Warning($"BACKUP CANCELLED | {job.Name} | {ex.Message}");
            return result;
        }
        catch (Exception ex)
        {
            result.Status = BackupStatus.Failed;
            result.Errors.Add(ex.Message);
            _loggingService.Error($"BACKUP FAILED | {job.Name}", ex);
            return result;
        }
    }

    public static string CreateBackupSetName()
    {
        var now = DateTime.Now;
        return $"Backup_{now:yyyy-MM-dd}_{now:HHmmss}_{now:fff}";
    }

    public static bool IsBackupSetDirectoryName(string directoryName)
    {
        return BackupSetRegex.IsMatch(directoryName);
    }

    private sealed class FileMetadata
    {
        public long Length { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }
}
