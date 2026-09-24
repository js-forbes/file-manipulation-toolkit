namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IFileSystemService
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> EnumerateFiles(string path, bool recursive = true);
    IEnumerable<string> EnumerateDirectories(string path, bool recursive = true);
    void CreateDirectory(string path);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive = false);
    string GetFullPath(string path);
    bool PathsAreEquivalent(string first, string second);
    bool IsPathInsideDirectory(string candidatePath, string rootPath);
    FileAttributes GetAttributes(string path);
    DateTime GetLastWriteTimeUtc(string path);
    long GetLength(string path);
}
