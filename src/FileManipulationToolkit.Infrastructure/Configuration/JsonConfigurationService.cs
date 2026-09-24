using System.Text.Json;
using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.Infrastructure.Configuration;

public class JsonConfigurationService : IConfigurationService
{
    private readonly string _configPath;

    public JsonConfigurationService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appData, "FileManipulationToolkit");
        Directory.CreateDirectory(appDirectory);
        _configPath = Path.Combine(appDirectory, "config.json");
    }

    public string GetConfigPath() => _configPath;

    public ApplicationConfig Load()
    {
        if (!File.Exists(_configPath))
        {
            return new ApplicationConfig();
        }

        var json = File.ReadAllText(_configPath);
        var config = JsonSerializer.Deserialize<ApplicationConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return config ?? new ApplicationConfig();
    }

    public void Save(ApplicationConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? Environment.CurrentDirectory);
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_configPath, json);
    }
}
