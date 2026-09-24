using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Deletion;

public class FileDeletionService : IFileDeletionService
{
    public async Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
            {
                return false;
            }

            await Task.Run(() => File.Delete(path), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteEmptyDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(path))
            {
                return false;
            }

            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                return false;
            }

            await Task.Run(() => Directory.Delete(path), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
