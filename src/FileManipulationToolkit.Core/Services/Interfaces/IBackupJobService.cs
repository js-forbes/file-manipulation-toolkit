using FileManipulationToolkit.Core.Domain.Models;

namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IBackupJobService
{
    List<BackupJob> GetJobs();
    void AddJob(BackupJob job);
    void UpdateJob(BackupJob job);
    void DeleteJob(Guid id);
    BackupJob? GetById(Guid id);
}
