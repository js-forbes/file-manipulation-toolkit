using FileManipulationToolkit.Core.Domain.Enums;

namespace FileManipulationToolkit.Core.Domain.Models;

public class GlobalSettings
{
    public int RetentionCount { get; set; } = 3;
    public BackupOperationMode OperationMode { get; set; } = BackupOperationMode.CopyOnly;
    public VerificationSettings Verification { get; set; } = new();
    public bool EnableScheduling { get; set; }
    public bool RunWithHighestPrivileges { get; set; }
}
