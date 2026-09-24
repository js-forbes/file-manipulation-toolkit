namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface ILoggingService
{
    void Information(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Debug(string message);
}
