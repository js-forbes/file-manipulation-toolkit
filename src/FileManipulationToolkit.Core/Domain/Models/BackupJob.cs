using FileManipulationToolkit.Core.Domain.Enums;

namespace FileManipulationToolkit.Core.Domain.Models;

public class BackupJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SourceFolder { get; set; } = string.Empty;
    public string BackupFolder { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int? RetentionCountOverride { get; set; }
    public BackupOperationMode? OperationModeOverride { get; set; }
    public bool UseGlobalSetting { get; set; } = true;
    public string LastStatus { get; set; } = string.Empty;
    public DateTime? LastRunUtc { get; set; }
}
