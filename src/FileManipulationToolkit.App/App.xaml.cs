using System.Windows;
using FileManipulationToolkit.App.ViewModels;
using FileManipulationToolkit.Core.Services.Interfaces;
using FileManipulationToolkit.Infrastructure.Configuration;
using FileManipulationToolkit.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace FileManipulationToolkit.App;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddSingleton<IConfigurationService, JsonConfigurationService>();
        services.AddSingleton<ILoggingService, FileLogger>();
        services.AddSingleton<MainViewModel>();

        Services = services.BuildServiceProvider();

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };

        mainWindow.Show();
    }
}

