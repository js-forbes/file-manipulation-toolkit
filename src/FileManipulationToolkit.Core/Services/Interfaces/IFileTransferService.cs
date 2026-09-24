namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileTransferService
{
    Task<string> MoveFileAsync(string sourceFile, string destinationDirectory, CancellationToken cancellationToken = default);
    Task<List<string>> MoveFilesAsync(string sourceDirectory, string destinationDirectory, CancellationToken cancellationToken = default);
}
