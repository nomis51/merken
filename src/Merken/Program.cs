using System.Reflection;
using Merken.Core.Models.Data;
using Merken.Core.Services;
using Merken.Core.Services.Abstractions;
using Merken.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Merken;

public static class Program
{
    #region Public methods

    public static async Task<int> Main(string[] _)
    {
        ConfigureLogging();

        try
        {
            Log.Information("Starting application...");
            var app = Host.CreateDefaultBuilder()
                .ConfigureServices(e => e.AddSerilog())
                .ConfigureServices(ConfigureServices)
                .ConfigureServices(e => e.AddSingleton<Application>())
                .Build();

            await app.Services
                .GetService<Application>()!
                .Run(
                    new MainView(
                        app.Services.GetService<IStorageService<Deck>>()!
                    ),
                    app.Services
                );
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Unexpected exception");
            return 1;
        }

        return 0;
    }

    #endregion

    #region Private methods

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                Path.Join(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "logs",
                    ".txt"
                ),
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddSingleton<MainView>();
        services.AddSingleton<DeckView>();
        services.AddSingleton<EditDeckView>();
        services.AddSingleton<EditCardView>();
        services.AddSingleton<BrowseCardsView>();
        services.AddSingleton<StudyView>();

        services.AddScoped(typeof(IStorageService<>), typeof(StorageService<>));
        services.AddScoped<IFsrsService, FsrsService>();
        services.AddScoped<IGitService, GitService>();
    }

    #endregion
}