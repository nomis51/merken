using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class EditDeckView : IView
{
    #region Members

    private readonly IStorageService<Deck> _deckStorageService;

    #endregion

    #region Constructors

    public EditDeckView(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? args = null)
    {
        var isNewDeck = args is null;
        AnsiConsole.Write(
            new Rule(isNewDeck ? "Create a new deck" : "Edit deck")
                .LeftJustified()
        );
        if (isNewDeck)
        {
            var id = await CreateDeck();
            return new ViewResult(typeof(DeckView), id);
        }

        await EditDeck((Guid)args!);
        return new ViewResult(typeof(DeckView), args);
    }

    #endregion

    #region Private methods

    private async Task<Guid?> CreateDeck()
    {
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Deck name:")
                .AllowEmpty()
        );
        if (string.IsNullOrEmpty(name)) return null;

        var deck = new Deck
        {
            Name = name
        };
        await _deckStorageService.AddAsync(deck);
        return deck.Id;
    }

    private async Task EditDeck(Guid id)
    {
        // TODO: get deck from db
        var deck = await _deckStorageService.GetByIdAsync(id);
        if (deck is null) return;


        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Deck name:")
                .DefaultValue(deck.Name)
        );
        if (string.IsNullOrEmpty(name)) return;

        deck.Name = name;
        await _deckStorageService.UpdateAsync(deck);
    }

    #endregion
}