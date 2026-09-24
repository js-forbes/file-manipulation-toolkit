namespace FileManipulationToolkit.Core.Domain.Enums;

public enum BackupPhase
{
    Preparing = 0,
    Copying = 1,
    Verifying = 2,
    Deleting = 3,
    Retention = 4,
    Completed = 5,
    Cancelled = 6,
    Failed = 7
}
