using System.Security.Cryptography;
using FileManipulationToolkit.Core.Domain.Enums;
using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Domain.Results;
using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Verification;

public class FileVerificationService : IFileVerificationService
{
    private const int DefaultBufferSize = 1024 * 1024;

    public async Task<VerificationResult> VerifyAsync(
        string sourcePath,
        string destinationPath,
        bool verifyFileExists = true,
        bool compareSize = true,
        bool useSha256 = true,
        CancellationToken cancellationToken = default,
        IProgress<BackupProgress>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("Destination path is required.", nameof(destinationPath));
        }

        if (!File.Exists(sourcePath))
        {
            return new VerificationResult
            {
                Passed = false,
                FilePath = sourcePath,
                Exists = false,
                FailureReason = $"Source file does not exist: {sourcePath}"
            };
        }

        if (verifyFileExists && !File.Exists(destinationPath))
        {
            return new VerificationResult
            {
                Passed = false,
                FilePath = destinationPath,
                Exists = false,
                FailureReason = $"Destination file does not exist: {destinationPath}"
            };
        }

        cancellationToken.ThrowIfCancellationRequested();

        var sourceInfo = new FileInfo(sourcePath);
        var destinationInfo = new FileInfo(destinationPath);
        var sourceLength = sourceInfo.Length;
        var destinationLength = destinationInfo.Length;

        var result = new VerificationResult
        {
            FilePath = destinationPath,
            Exists = true,
            SourceLength = sourceLength,
            DestinationLength = destinationLength,
            SizeMatches = !compareSize || sourceLength == destinationLength
        };

        if (compareSize && !result.SizeMatches)
        {
            result.Passed = false;
            result.FailureReason = $"File size mismatch: source={sourceLength} destination={destinationLength}";
            return result;
        }

        if (useSha256)
        {
            var sourceHash = await ComputeHashAsync(sourcePath, cancellationToken, progress, "source");
            var destinationHash = await ComputeHashAsync(destinationPath, cancellationToken, progress, "destination");

            result.SourceHash = sourceHash;
            result.DestinationHash = destinationHash;
            result.HashMatches = string.Equals(sourceHash, destinationHash, StringComparison.OrdinalIgnoreCase);

            if (!result.HashMatches)
            {
                result.Passed = false;
                result.FailureReason = "SHA-256 hash mismatch between source and destination.";
                return result;
            }
        }

        result.Passed = true;
        result.FailureReason = null;
        return result;
    }

    private static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken, IProgress<BackupProgress>? progress, string label)
    {
        await using var stream = File.OpenRead(path);
        var totalBytes = stream.Length;
        long processedBytes = 0;

        using var sha256 = SHA256.Create();
        var buffer = new byte[DefaultBufferSize];

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var count = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (count == 0)
            {
                break;
            }

            processedBytes += count;

            progress?.Report(new BackupProgress
            {
                JobName = label,
                Phase = BackupPhase.Verifying,
                CurrentFile = path,
                CurrentFileSize = totalBytes,
                TotalFiles = 1,
                BytesCompleted = processedBytes,
                TotalBytes = totalBytes,
                Elapsed = TimeSpan.Zero,
                EstimatedRemaining = TimeSpan.Zero
            });
        }

        stream.Position = 0;
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
