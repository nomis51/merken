using Merken.Core.Enums;
using Merken.Core.Models.Data;

namespace Merken.Core.Models.Fsrs;

public class SchedulingCards
{
    #region Props

    public Card Again { get; } = new();
    public Card Hard { get; } = new();
    public Card Good { get; } = new();
    public Card Easy { get; } = new();

    #endregion

    #region Constructors

    public SchedulingCards(Card card)
    {
        Again.Question = card.Question;
        Hard.Question = card.Question;
        Good.Question = card.Question;
        Easy.Question = card.Question;

        Again.Answer = card.Answer;
        Hard.Answer = card.Answer;
        Good.Answer = card.Answer;
        Easy.Answer = card.Answer;

        Again.NbMistakes = card.NbMistakes;
        Hard.NbMistakes = card.NbMistakes;
        Good.NbMistakes = card.NbMistakes;
        Easy.NbMistakes = card.NbMistakes;

        Again.NbRepetitions = card.NbRepetitions;
        Hard.NbRepetitions = card.NbRepetitions;
        Good.NbRepetitions = card.NbRepetitions;
        Easy.NbRepetitions = card.NbRepetitions;

        Again.LastReviewAt = card.LastReviewAt;
        Hard.LastReviewAt = card.LastReviewAt;
        Good.LastReviewAt = card.LastReviewAt;
        Easy.LastReviewAt = card.LastReviewAt;

        Again.ElapsedDays = card.ElapsedDays;
        Hard.ElapsedDays = card.ElapsedDays;
        Good.ElapsedDays = card.ElapsedDays;
        Easy.ElapsedDays = card.ElapsedDays;

        Again.State = card.State;
        Hard.State = card.State;
        Good.State = card.State;
        Easy.State = card.State;
    }

    #endregion

    #region Public methods

    public void UpdateState(State state)
    {
        switch (state)
        {
            case State.New:
                Again.State = State.Learning;
                Hard.State = State.Learning;
                Good.State = State.Learning;
                Easy.State = State.Review;
                break;

            case State.Learning:
            case State.Relearning:
                Again.State = state;
                Hard.State = state;
                Good.State = State.Review;
                Easy.State = State.Review;
                break;

            case State.Review:
                Again.State = State.Relearning;
                Hard.State = State.Review;
                Good.State = State.Review;
                Easy.State = State.Review;
                Again.NbMistakes += 1;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void Schedule(DateTime now, int hardInterval, int goodInterval, int easyInterval)
    {
        Again.ScheduledDays = 0;
        Hard.ScheduledDays = hardInterval;
        Good.ScheduledDays = goodInterval;
        Easy.ScheduledDays = easyInterval;

        Again.DueAt = now.AddMinutes(5);
        Hard.DueAt = hardInterval > 0 ? now.AddDays(hardInterval) : now.AddMinutes(10);

        Good.DueAt = now.AddDays(goodInterval);
        Easy.DueAt = now.AddDays(easyInterval);
    }

    public Dictionary<Rating, SchedulingInfo> RecordLog(Card card, DateTime now)
    {
        return new Dictionary<Rating, SchedulingInfo>
        {
            {
                Rating.Again,
                new SchedulingInfo(Again,
                    new ReviewLog(Rating.Again, Again.ScheduledDays, card.ElapsedDays, now, card.State))
            },
            {
                Rating.Hard,
                new SchedulingInfo(Hard,
                    new ReviewLog(Rating.Hard, Hard.ScheduledDays, card.ElapsedDays, now, card.State))
            },
            {
                Rating.Good,
                new SchedulingInfo(Good,
                    new ReviewLog(Rating.Good, Good.ScheduledDays, card.ElapsedDays, now, card.State))
            },
            {
                Rating.Easy,
                new SchedulingInfo(Easy,
                    new ReviewLog(Rating.Easy, Easy.ScheduledDays, card.ElapsedDays, now, card.State))
            },
        };
    }

    #endregion
}