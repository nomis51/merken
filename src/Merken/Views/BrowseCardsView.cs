using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class BrowseCardsView : IView
{
    #region Constants

    private readonly Keybind[] _keybinds =
    [
        new Keybind("Enter", "Edit"),
        new Keybind("n", "New"),
        new Keybind("d", "Delete"),
        new Keybind("j / ↓", "Move down"),
        new Keybind("k / ↑", "Move up"),
        new Keybind("h", "Help"),
        new Keybind("q", "Back")
    ];

    private readonly string[] _headers =
    [
        "Question",
        "Answer"
    ];

    private int _selectedCardIndex;

    #endregion

    #region Members

    private readonly IStorageService<Deck> _deckStorageService;

    #endregion

    #region Constructors

    public BrowseCardsView(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? args = null)
    {
        try
        {
            if (args is not Guid deckId)
            {
                return new ViewResult(typeof(DeckView), args);
            }

            Console.CursorVisible = false;

            var deck = await GetDeck(deckId);
            if (deck is null)
            {
                return new ViewResult(typeof(DeckView), deckId);
            }

            while (true)
            {
                Console.SetCursorPosition(0, 0);

                var cardTable = new Table()
                    .Centered()
                    .AddColumns(
                        _headers.Select(
                                e => new TableColumn(e)
                                    .LeftAligned()
                            )
                            .ToArray()
                    );
                for (var i = 0; i < deck.Cards.Count; ++i)
                {
                    if (_selectedCardIndex == i)
                    {
                        cardTable.AddRow(
                            new Markup($"[blue]{deck.Cards[i].Question}[/]"),
                            new Markup($"[blue]{deck.Cards[i].Answer}[/]")
                        );
                    }
                    else
                    {
                        cardTable.AddRow(
                            new Text(deck.Cards[i].Question),
                            new Text(deck.Cards[i].Answer)
                        );
                    }
                }

                new Layout()
                    .Render(cardTable, _keybinds, deck.Cards.Count + 4);

                var choice = Console.ReadKey(true);

                switch (choice.Key)
                {
                    case ConsoleKey.Enter:
                        if (_selectedCardIndex > deck.Cards.Count - 1) break;

                        return new ViewResult(typeof(EditCardView),
                            (typeof(BrowseCardsView), deckId, deck.Cards[_selectedCardIndex].Question));

                    case ConsoleKey.N:
                        return new ViewResult(typeof(EditCardView), (typeof(BrowseCardsView), deckId, (string)null!));

                    case ConsoleKey.D:
                        if (_selectedCardIndex > deck.Cards.Count - 1) break;

                        var confirmDelete =
                            AnsiConsole.Confirm(
                                $"Are you sure you want to [red]delete[/] the card [blue]{deck.Cards[_selectedCardIndex].Question}[/]?",
                                false);
                        if (!confirmDelete) break;

                        deck.Cards.RemoveAt(_selectedCardIndex);
                        await _deckStorageService.UpdateAsync(deck);
                        break;

                    case ConsoleKey.J:
                    case ConsoleKey.DownArrow:
                        if (_selectedCardIndex > deck.Cards.Count - 1) break;

                        _selectedCardIndex = (_selectedCardIndex + 1) % deck.Cards.Count;
                        break;

                    case ConsoleKey.K:
                    case ConsoleKey.UpArrow:
                        if (_selectedCardIndex > deck.Cards.Count - 1) break;

                        _selectedCardIndex = (_selectedCardIndex - 1 + deck.Cards.Count) % deck.Cards.Count;
                        break;

                    case ConsoleKey.Q:
                        return new ViewResult(typeof(DeckView), deckId);
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

    private async Task<Deck?> GetDeck(Guid id)
    {
        return await _deckStorageService.GetByIdAsync(id);
    }

    #endregion
}