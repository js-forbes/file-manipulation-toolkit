namespace FileManipulationToolkit.Core.Domain.Models;

public class VerificationSettings
{
    public bool VerifyCopiedFiles { get; set; } = true;
    public bool UseSHA256 { get; set; } = true;
    public bool CompareFileSizes { get; set; } = true;
    public bool VerifyDestinationExists { get; set; } = true;
}
