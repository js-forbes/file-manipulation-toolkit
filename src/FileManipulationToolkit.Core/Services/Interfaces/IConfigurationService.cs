using FileManipulationToolkit.Core.Domain.Models;

namespace FileManipulationToolkit.Core.Services.Interfaces;

public interface IConfigurationService
{
    ApplicationConfig Load();
    void Save(ApplicationConfig config);
    string GetConfigPath();
}
