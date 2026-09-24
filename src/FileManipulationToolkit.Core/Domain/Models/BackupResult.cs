using FileManipulationToolkit.Core.Domain.Enums;

namespace FileManipulationToolkit.Core.Domain.Models;

public class BackupResult
{
    public BackupStatus Status { get; set; }
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public int DeletedFiles { get; set; }
    public int DeleteFailedFiles { get; set; }
    public long TotalBytes { get; set; }
    public long ProcessedBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
