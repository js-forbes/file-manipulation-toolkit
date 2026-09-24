namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface ISchedulerService
{
    void CreateOrUpdateTask(string taskName, string operation, bool runWithHighestPrivileges);
    void RemoveTask(string taskName);
    bool IsEnabled(string taskName);
}
