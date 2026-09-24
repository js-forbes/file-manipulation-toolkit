using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Logging;

public class FileLogger : ILoggingService
{
    private readonly string _logDirectory;
    private readonly string _logPath;

    public FileLogger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _logDirectory = Path.Combine(appData, "FileManipulationToolkit", "Logs");
        Directory.CreateDirectory(_logDirectory);
        _logPath = Path.Combine(_logDirectory, $"file-manipulation-toolkit-{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public void Information(string message) => WriteLog("INFO", message);

    public void Warning(string message) => WriteLog("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        if (exception is not null)
        {
            message = $"{message} | {exception.GetType().Name}: {exception.Message}";
        }

        WriteLog("ERROR", message);
    }

    public void Debug(string message) => WriteLog("DEBUG", message);

    private void WriteLog(string level, string message)
    {
        var line = $"{DateTime.UtcNow:O} [{level}] {message}";
        File.AppendAllText(_logPath, line + Environment.NewLine);
    }
}
