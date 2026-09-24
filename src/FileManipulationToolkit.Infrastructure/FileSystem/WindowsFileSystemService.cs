using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.FileSystem;

public class WindowsFileSystemService : IFileSystemService
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public IEnumerable<string> EnumerateFiles(string path, bool recursive = true)
    {
        if (!DirectoryExists(path))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    public IEnumerable<string> EnumerateDirectories(string path, bool recursive = true)
    {
        if (!DirectoryExists(path))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateDirectories(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    public string GetFullPath(string path) => Path.GetFullPath(path);

    public bool PathsAreEquivalent(string first, string second)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
        {
            return false;
        }

        return string.Equals(
            Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
    }

    public bool IsPathInsideDirectory(string candidatePath, string rootPath)
    {
        if (string.IsNullOrWhiteSpace(candidatePath) || string.IsNullOrWhiteSpace(rootPath))
        {
            return false;
        }

        var fullCandidate = Path.GetFullPath(candidatePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return fullCandidate.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)
            && !fullCandidate.Equals(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    public FileAttributes GetAttributes(string path)
    {
        if (File.Exists(path))
        {
            return File.GetAttributes(path);
        }

        if (Directory.Exists(path))
        {
            return new DirectoryInfo(path).Attributes;
        }

        throw new FileNotFoundException($"The path '{path}' does not exist.", path);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        if (File.Exists(path))
        {
            return File.GetLastWriteTimeUtc(path);
        }

        if (Directory.Exists(path))
        {
            return Directory.GetLastWriteTimeUtc(path);
        }

        throw new FileNotFoundException($"The path '{path}' does not exist.", path);
    }

    public long GetLength(string path)
    {
        if (File.Exists(path))
        {
            return new FileInfo(path).Length;
        }

        return 0;
    }
}
