using Merken.Core.Enums;
using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class StudyView : IView
{
    #region Constants

    private readonly Keybind[] _keybinds =
    [
        new Keybind("1", "Again", "red"),
        new Keybind("2", "Hard", "yellow"),
        new Keybind("3", "Good", "blue"),
        new Keybind("4", "Easy", "green"),
        new Keybind("Any", "Reveal"),
        new Keybind("q", "Quit"),
    ];

    #endregion

    #region Members

    private readonly IStorageService<Deck> _deckStorageService;
    private readonly IFsrsService _fsrsService;

    #endregion

    #region Constructors

    public StudyView(IStorageService<Deck> deckStorageService, IFsrsService fsrsService)
    {
        _deckStorageService = deckStorageService;
        _fsrsService = fsrsService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? args = null)
    {
        if (args is not Guid deckId)
        {
            return new ViewResult(typeof(DeckView), args);
        }

        var deck = await _deckStorageService.GetByIdAsync(deckId);
        if (deck is null)
        {
            return new ViewResult(typeof(DeckView), args);
        }

        var layout = new Layout();

        while (true)
        {
            Console.SetCursorPosition(0, 0);

            var card = deck.Cards
                .Where(e => e.DueAt.Date <= DateTime.Now.Date)
                .OrderBy(e => e.LastReviewAt)
                .ThenBy(e => e.DueAt)
                .FirstOrDefault();
            if (card is null) break;

            var cardScheduling = _fsrsService.Repeat(card);

            var rows = new Rows(
                    new Padder(
                            new Text(card.Question)
                                .Centered(),
                            new Padding(0, 1)
                        )
                        .Expand(),
                    new Rule(),
                    new Padder(
                            new Text(string.Empty)
                                .Centered(),
                            new Padding(0, 1)
                        )
                        .Expand()
                )
                .Expand();

            layout.Render(rows, _keybinds, 7);

            var choice = Console.ReadKey(true);
            if (choice.Key is ConsoleKey.Q) break;

            Console.SetCursorPosition(0, 0);
            rows = new Rows(
                    new Padder(
                            new Text(card.Question)
                                .Centered(),
                            new Padding(0, 1)
                        )
                        .Expand(),
                    new Rule(),
                    new Padder(
                            new Text(card.Answer)
                                .Centered(),
                            new Padding(0, 1)
                        )
                        .Expand()
                )
                .Expand();
            layout.Render(rows, _keybinds, 7);

            while (true)
            {
                choice = Console.ReadKey(true);
                if (choice.Key is ConsoleKey.Q) break;
                if ((int)choice.KeyChar is >= 49 and <= 52) break;
            }

            if (choice.Key is ConsoleKey.Q) break;

            var info = cardScheduling[(Rating)choice.KeyChar - 48];
            deck.Cards.Remove(card);
            deck.Cards.Add(info.Card);

            await _deckStorageService.UpdateAsync(deck);
        }

        return new ViewResult(typeof(DeckView), deckId);
    }

    #endregion
}