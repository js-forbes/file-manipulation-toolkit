using FileManipulationToolkit.Core.Domain.Enums;

namespace FileManipulationToolkit.Core.Domain.Models;

public class ScheduleSettings
{
    public bool Enabled { get; set; }
    public ScheduleType Type { get; set; } = ScheduleType.Daily;
    public TimeOnly ExecutionTime { get; set; } = new(2, 0);
    public List<DayOfWeek> WeeklyDays { get; set; } = new();
}
