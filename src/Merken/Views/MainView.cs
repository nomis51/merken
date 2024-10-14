using Merken.Core.Enums;
using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class MainView : IView
{
    #region Constants

    private readonly string[] _headers =
    [
        "Deck",
        "Cards",
        "New",
        "Learning",
        "Review"
    ];

    private readonly Keybind[] _keybinds =
    [
        new Keybind("Enter", "Show"),
        new Keybind("n", "New"),
        new Keybind("d", "Delete"),
        new Keybind("j / ↓", "Move down"),
        new Keybind("k / ↑", "Move up"),
        new Keybind("h", "Help"),
        new Keybind("q", "Quit"),
    ];

    #endregion

    #region Members

    private readonly IStorageService<Deck> _deckStorageService;
    private int _selectedDeck;

    #endregion

    #region Constructors

    public MainView(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? _ = null)
    {
        try
        {
            Console.CursorVisible = false;

            var decks = await GetDecks();

            while (true)
            {
                Console.SetCursorPosition(0, 0);

                var deckTable = new Table()
                    .Centered()
                    .NoBorder()
                    .HorizontalBorder()
                    .AddColumns(
                        _headers.Select(
                                e => new TableColumn(e)
                                    .LeftAligned()
                            )
                            .ToArray()
                    );
                for (var i = 0; i < decks.Count; ++i)
                {
                    if (_selectedDeck == i)
                    {
                        deckTable.AddRow(
                            new Markup($"[blue]{decks[i].Name}[/]"),
                            new Markup($"[blue]{decks[i].Cards.Count}[/]"),
                            new Markup($"[blue]{decks[i].Cards.Count(e => e.State is State.New)}[/]"),
                            new Markup(
                                $"[blue]{decks[i].Cards.Count(e => e.State is State.Learning or State.Relearning)}[/]"),
                            new Markup($"[blue]{decks[i].Cards.Count(e => e.State is State.Review)}[/]")
                        );
                    }
                    else
                    {
                        deckTable.AddRow(
                            new Text(decks[i].Name),
                            new Text(decks[i].Cards.Count.ToString()),
                            new Text(decks[i].Cards.Count(e => e.State is State.New).ToString()),
                            new Text(
                                decks[i].Cards.Count(e => e.State is State.Learning or State.Relearning).ToString()),
                            new Text(decks[i].Cards.Count(e => e.State is State.Review).ToString())
                        );
                    }
                }

                new Layout()
                    .Render(deckTable, _keybinds, decks.Count);

                var choice = Console.ReadKey(true);

                switch (choice.Key)
                {
                    case ConsoleKey.Enter:
                        if (_selectedDeck > decks.Count - 1) break;

                        return new ViewResult(typeof(DeckView), decks[_selectedDeck].Id);

                    case ConsoleKey.N:
                        return new ViewResult(typeof(EditDeckView));

                    case ConsoleKey.D:
                        if (_selectedDeck > decks.Count - 1) break;

                        Console.Clear();
                        var confirmDelete =
                            AnsiConsole.Confirm(
                                $"Are you sure you want to [red]delete[/] the deck [blue]{decks[_selectedDeck].Name}[/]?",
                                false);
                        if (!confirmDelete) break;

                        await _deckStorageService.DeleteAsync(decks[_selectedDeck].Id);
                        decks = await GetDecks();

                        if (decks.Count == 0)
                        {
                            _selectedDeck = 0;
                            break;
                        }

                        _selectedDeck = (_selectedDeck + 1) % decks.Count;
                        break;

                    case ConsoleKey.J:
                    case ConsoleKey.DownArrow:
                        if (_selectedDeck > decks.Count - 1) break;

                        _selectedDeck = (_selectedDeck + 1) % decks.Count;
                        break;

                    case ConsoleKey.K:
                    case ConsoleKey.UpArrow:
                        if (_selectedDeck > decks.Count - 1) break;

                        _selectedDeck = (_selectedDeck - 1 + decks.Count) % decks.Count;
                        break;

                    case ConsoleKey.Q:
                        return new ViewResult(null);
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    #endregion

    #region Private methods

    private async Task<List<Deck>> GetDecks()
    {
        return await _deckStorageService.GetAllAsync();
    }

    #endregion
}