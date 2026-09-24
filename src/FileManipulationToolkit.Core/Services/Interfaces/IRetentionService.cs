namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IRetentionService
{
    Task ApplyRetentionAsync(string backupRoot, int retentionCount, CancellationToken cancellationToken = default);
}
