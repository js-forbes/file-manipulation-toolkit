using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Domain.Results;

namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileVerificationService
{
    Task<VerificationResult> VerifyAsync(
        string sourcePath,
        string destinationPath,
        bool verifyFileExists = true,
        bool compareSize = true,
        bool useSha256 = true,
        CancellationToken cancellationToken = default,
        IProgress<BackupProgress>? progress = null);
}
