using FileManipulationToolkit.Core.Domain.Enums;

namespace FileManipulationToolkit.Core.Domain.Models;

public class BackupProgress
{
    public string JobName { get; set; } = string.Empty;
    public BackupPhase Phase { get; set; }
    public int CurrentFileNumber { get; set; }
    public int TotalFiles { get; set; }
    public long BytesCompleted { get; set; }
    public long TotalBytes { get; set; }
    public string CurrentFile { get; set; } = string.Empty;
    public long CurrentFileSize { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public TimeSpan Elapsed { get; set; }
    public TimeSpan EstimatedRemaining { get; set; }
}
