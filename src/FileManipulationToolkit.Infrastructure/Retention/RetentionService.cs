using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Retention;

public class RetentionService : IRetentionService
{
    public Task ApplyRetentionAsync(string backupRoot, int retentionCount, CancellationToken cancellationToken = default)
    {
        if (retentionCount < 1)
        {
            return Task.CompletedTask;
        }

        if (!Directory.Exists(backupRoot))
        {
            return Task.CompletedTask;
        }

        var entries = Directory.EnumerateDirectories(backupRoot)
            .Select(path => new
            {
                Path = path,
                Name = Path.GetFileName(path)
            })
            .Where(x => x.Name != null && x.Name.StartsWith("Backup_", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var entry in entries.Skip(retentionCount))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Delete(entry.Path, true);
        }

        return Task.CompletedTask;
    }
}
