using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions.DependencyInjection;
using HaselCommon.Logger;
using LazyLeves.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LazyLeves.Windows;
using HaselCommon.Services;

namespace LazyLeves;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IFramework framework,
        IPluginLog pluginLog,
        ISigScanner sigScanner,
        IDataManager dataManager)
    {
        Service
            // Dalamud & HaselCommon
            .Initialize(pluginInterface, pluginLog)

            // Logging
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(new DalamudLoggerProvider(pluginLog));
            })

            // Config
            .AddSingleton(PluginConfig.Load(pluginInterface, pluginLog))

            // Windows
            .AddSingleton<MainWindow>()
            .AddSingleton<ConfigWindow>()
            .AddSingleton<FiltersState>()
            .AddSingleton<ExcelService>();

        Service.BuildProvider();

        framework.RunOnFrameworkThread(() =>
        {
            Service.Get<MainWindow>();
        });
    }

    void IDisposable.Dispose()
    {
        Service.Dispose();
    }
}
