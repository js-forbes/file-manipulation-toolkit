using FileManipulationToolkit.Core.Domain.Models;
using FileManipulationToolkit.Core.Services.Interfaces;

namespace FileManipulationToolkit.App.ViewModels;

public class MainViewModel
{
    private readonly IConfigurationService _configurationService;
    private readonly ILoggingService _loggingService;

    public MainViewModel(IConfigurationService configurationService, ILoggingService loggingService)
    {
        _configurationService = configurationService;
        _loggingService = loggingService;

        Config = _configurationService.Load();
        Title = "File Manipulation Toolkit";
        _loggingService.Information("Application startup");
    }

    public string Title { get; set; }

    public ApplicationConfig Config { get; private set; }
}
