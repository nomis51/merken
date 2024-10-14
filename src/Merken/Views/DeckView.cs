using Merken.Core.Enums;
using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class DeckView : IView
{
    #region Constants

    private readonly Keybind[] _keybinds =
    [
        new Keybind("s", "Study"),
        new Keybind("a", "Add"),
        new Keybind("b", "Browse"),
        new Keybind("e", "Edit"),
        new Keybind("h", "Help"),
        new Keybind("q", "Back"),
    ];

    #endregion

    #region Members

    private readonly IStorageService<Deck> _deckStorageService;

    #endregion

    #region Constructors

    public DeckView(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? args = null)
    {
        if (args is not Guid) return new ViewResult(typeof(MainView));
        var deckId = (Guid)args;

        var deck = await GetDeck(deckId);
        if (deck is null) return new ViewResult(typeof(MainView));

        while (true)
        {
            Console.Clear();

            var table = new Table()
                .AddColumns(
                    new TableColumn(new Markup($"[white]{deck.Name}[/]"))
                        .Centered()
                )
                .Centered()
                .NoBorder()
                .HorizontalBorder()
                .AddRow(
                    new Table()
                        .Centered()
                        .NoBorder()
                        .HideHeaders()
                        .AddColumns(string.Empty, string.Empty)
                        .AddRow(
                            new Text("Cards: ").LeftJustified(),
                            new Markup($"[white]{deck.Cards.Count}[/]")
                        )
                        .AddRow(
                            new Text("New: ").LeftJustified(),
                            new Markup($"[blue]{deck.Cards.Count(e => e.State is State.New)}[/]")
                        )
                        .AddRow(
                            new Text("Learning: ").LeftJustified(),
                            new Markup(
                                $"[red]{deck.Cards.Count(e => e.State is State.Learning or State.Relearning)}[/]")
                        )
                        .AddRow(
                            new Text("To review: ").LeftJustified(),
                            new Markup($"[green]{deck.Cards.Count(e => e.State == State.Review)}[/]")
                        )
                );

            new Layout()
                .Render(table, _keybinds, 8);

            var choice = Console.ReadKey(true);

            switch (choice.Key)
            {
                case ConsoleKey.Q:
                    return new ViewResult(typeof(MainView));

                case ConsoleKey.E:
                    return new ViewResult(typeof(EditDeckView), deckId);

                case ConsoleKey.A:
                    return new ViewResult(typeof(EditCardView), (typeof(DeckView), deckId, (string)null!));

                case ConsoleKey.B:
                    return new ViewResult(typeof(BrowseCardsView), deckId);

                case ConsoleKey.S:
                    return new ViewResult(typeof(StudyView), deckId);
            }
        }
    }

    #endregion

    #region Private methods

    private async Task<Deck?> GetDeck(Guid id)
    {
        return await _deckStorageService.GetByIdAsync(id);
    }

    #endregion
}