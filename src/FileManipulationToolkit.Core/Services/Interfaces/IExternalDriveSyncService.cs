namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IExternalDriveSyncService
{
    Task<List<string>> MirrorToDriveAsync(string sourceDirectory, string driveRoot, string mirrorFolderName, CancellationToken cancellationToken = default);
    Task<List<string>> MirrorFromDriveAsync(string driveRoot, string destinationDirectory, string mirrorFolderName, CancellationToken cancellationToken = default);
}
