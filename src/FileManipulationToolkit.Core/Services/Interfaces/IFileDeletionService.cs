namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileDeletionService
{
    Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmptyDirectoryAsync(string path, CancellationToken cancellationToken = default);
}
