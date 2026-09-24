namespace FileManipulationToolkit.Core.Domain.Results;

public class VerificationResult
{
    public bool Passed { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public bool SizeMatches { get; set; }
    public bool HashMatches { get; set; }
    public string? FailureReason { get; set; }
    public long SourceLength { get; set; }
    public long DestinationLength { get; set; }
    public string? SourceHash { get; set; }
    public string? DestinationHash { get; set; }
}
