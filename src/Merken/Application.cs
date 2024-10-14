using System.Text;
using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Views.Abstractions;
using Serilog;
using Spectre.Console;

namespace Merken;

public class Application
{
    #region Members

    private readonly IStorageService<Deck> _deckStorageService;

    #endregion

    #region Constructors

    public Application(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task Run(IView? initialView, IServiceProvider services)
    {
        Console.OutputEncoding = Encoding.Unicode;

        if (!await _deckStorageService.Sync())
        {
            Log.Warning("Failed to sync");
            AnsiConsole.Markup("[yellow]Failed to sync[/]");
            Console.ReadKey(true);
        }

        if (OperatingSystem.IsWindows())
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
        }

        object? args = null;

        while (initialView is not null)
        {
            Console.Clear();
            var (viewType, a) = await initialView.Render(args);
            if (viewType is null) break;

            args = a;
            initialView = (IView)services.GetService(viewType)!;
        }

        await _deckStorageService.Sync();
    }

    #endregion
}