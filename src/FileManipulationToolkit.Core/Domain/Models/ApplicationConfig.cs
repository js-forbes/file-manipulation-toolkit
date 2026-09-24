using System.Text.Json.Serialization;

namespace FileManipulationToolkit.Core.Domain.Models;

public class ApplicationConfig
{
    public string Version { get; set; } = "1.0.0";
    public GlobalSettings GlobalSettings { get; set; } = new();
    public ScheduleSettings ScheduleSettings { get; set; } = new();
    public List<BackupJob> Jobs { get; set; } = new();

    [JsonIgnore]
    public DateTime LastLoadedUtc { get; set; }
}
