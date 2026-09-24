using FileManipulationToolkit.Core.Domain.Models;

namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileCopyService
{
    Task CopyAsync(
        string sourcePath,
        string destinationPath,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
