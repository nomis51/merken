using Merken.Core.Enums;
using Merken.Core.Models.Data;
using Merken.Core.Services.Abstractions;
using Merken.Models;
using Merken.Views.Abstractions;
using Spectre.Console;

namespace Merken.Views;

public class EditCardView : IView
{
    #region Members

    private readonly IStorageService<Deck> _deckStorageService;

    #endregion

    #region Constructors

    public EditCardView(IStorageService<Deck> deckStorageService)
    {
        _deckStorageService = deckStorageService;
    }

    #endregion

    #region Public methods

    public async Task<ViewResult> Render(object? args = null)
    {
        var (toView, deckId, question) = ((Type, Guid, string?))args!;
        var isNewCard = question is null;
        AnsiConsole.Write(
            new Rule(isNewCard ? "Create a new card" : "Edit a card")
                .LeftJustified()
        );
        if (isNewCard)
        {
            await CreateCard(deckId);
        }
        else
        {
            await EditCard(deckId, question!);
        }

        return new ViewResult(toView, deckId);
    }

    #endregion

    #region Private methods

    private async Task CreateCard(Guid deckId)
    {
        var deck = await _deckStorageService.GetByIdAsync(deckId);
        if (deck is null) return;

        var question = AnsiConsole.Prompt(
            new TextPrompt<string>("Question:")
                .AllowEmpty()
        );
        if (string.IsNullOrEmpty(question)) return;

        var answer = AnsiConsole.Prompt(
            new TextPrompt<string>("Answer:")
                .AllowEmpty()
        );
        if (string.IsNullOrEmpty(answer)) return;

        var card = new Card
        {
            Question = question,
            Answer = answer,
            State = State.New,
        };

        deck.Cards.Add(card);
        await _deckStorageService.UpdateAsync(deck);
    }

    private async Task EditCard(Guid deckId, string question)
    {
        var deck = await _deckStorageService.GetByIdAsync(deckId);
        if (deck is null) return;

        var card = deck.Cards.FirstOrDefault(e => e.Question == question);
        if (card is null) return;

        var newQuestion = AnsiConsole.Prompt(
            new TextPrompt<string>("Question:")
                .DefaultValue(card.Question)
        );
        if (string.IsNullOrEmpty(newQuestion)) return;

        var newAnswer = AnsiConsole.Prompt(
            new TextPrompt<string>("Answer:")
                .DefaultValue(card.Answer)
        );
        if (string.IsNullOrEmpty(newAnswer)) return;

        card.Question = newQuestion;
        card.Answer = newAnswer;
        await _deckStorageService.UpdateAsync(deck);
    }

    #endregion
}