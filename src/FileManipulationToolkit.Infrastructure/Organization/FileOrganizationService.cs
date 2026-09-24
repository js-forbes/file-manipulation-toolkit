using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Organization;

public class FileOrganizationService : IFileOrganizationService
{
    private readonly IFileSystemService _fileSystemService;

    public FileOrganizationService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public Task<List<string>> OrganizeAsync(string sourceDirectory, string targetRoot, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(targetRoot))
        {
            throw new ArgumentException("Target root is required.", nameof(targetRoot));
        }

        if (!_fileSystemService.DirectoryExists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");
        }

        var movedFiles = new List<string>();
        var root = _fileSystemService.GetFullPath(sourceDirectory);
        var destinationRoot = _fileSystemService.GetFullPath(targetRoot);

        foreach (var file in _fileSystemService.EnumerateFiles(root, recursive: false).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var category = GetCategory(file);
            var destinationFolder = Path.Combine(destinationRoot, category);
            _fileSystemService.CreateDirectory(destinationFolder);

            var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
            var safeTarget = GetAvailableDestinationPath(destinationFile);
            File.Move(file, safeTarget);
            movedFiles.Add(safeTarget);
        }

        return Task.FromResult(movedFiles);
    }

    public Task<List<string>> DistributeAsync(string sourceDirectory, string targetRoot, int folderCount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(targetRoot))
        {
            throw new ArgumentException("Target root is required.", nameof(targetRoot));
        }

        if (folderCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(folderCount), "Folder count must be greater than zero.");
        }

        if (!_fileSystemService.DirectoryExists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");
        }

        var movedFiles = new List<string>();
        var sourceFiles = _fileSystemService.EnumerateFiles(sourceDirectory, recursive: false).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        for (var i = 1; i <= folderCount; i++)
        {
            _fileSystemService.CreateDirectory(Path.Combine(targetRoot, $"Target_{i}"));
        }

        for (var index = 0; index < sourceFiles.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var targetFolderIndex = index % folderCount;
            var targetDirectory = Path.Combine(_fileSystemService.GetFullPath(targetRoot), $"Target_{targetFolderIndex + 1}");
            var destinationFile = Path.Combine(targetDirectory, Path.GetFileName(sourceFiles[index]));
            var safeTarget = GetAvailableDestinationPath(destinationFile);

            File.Move(sourceFiles[index], safeTarget);
            movedFiles.Add(safeTarget);
        }

        return Task.FromResult(movedFiles);
    }

    private static string GetCategory(string filePath)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        return extension switch
        {
            "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp" => "Images",
            "pdf" or "doc" or "docx" or "xls" or "xlsx" or "ppt" or "pptx" or "odt" => "Documents",
            "txt" or "csv" or "log" or "ini" or "json" or "xml" => "Text",
            "mp3" or "wav" or "flac" or "aac" => "Audio",
            "mp4" or "mov" or "avi" or "mkv" or "wmv" => "Video",
            _ => "Other"
        };
    }

    private static string GetAvailableDestinationPath(string destinationPath)
    {
        var candidate = destinationPath;
        var counter = 1;

        while (File.Exists(candidate))
        {
            var directory = Path.GetDirectoryName(candidate) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(candidate);
            var extension = Path.GetExtension(candidate);
            candidate = Path.Combine(directory, $"{fileName}_{counter}{extension}");
            counter++;
        }

        return candidate;
    }
}
