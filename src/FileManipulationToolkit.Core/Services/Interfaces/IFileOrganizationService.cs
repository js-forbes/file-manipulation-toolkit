namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileOrganizationService
{
    Task<List<string>> OrganizeAsync(string sourceDirectory, string targetRoot, CancellationToken cancellationToken = default);
    Task<List<string>> DistributeAsync(string sourceDirectory, string targetRoot, int folderCount, CancellationToken cancellationToken = default);
}
