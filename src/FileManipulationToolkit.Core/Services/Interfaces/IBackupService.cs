using FileManipulationToolkit.Core.Domain.Models;

namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IBackupService
{
    Task<BackupResult> RunAsync(CancellationToken cancellationToken, IProgress<BackupProgress>? progress = null);
    Task<BackupResult> RunJobAsync(BackupJob job, CancellationToken cancellationToken, IProgress<BackupProgress>? progress = null);
}
