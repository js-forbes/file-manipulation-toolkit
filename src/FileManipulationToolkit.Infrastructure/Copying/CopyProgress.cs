namespace FileManipulationToolkit.Infrastructure.Copying;

public class CopyProgress
{
    public long BytesCopied { get; set; }
    public long TotalBytes { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public double SpeedBytesPerSecond { get; set; }
    public TimeSpan Elapsed { get; set; }
    public TimeSpan EstimatedRemaining { get; set; }
}
