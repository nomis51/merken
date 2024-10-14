using Merken.Core.Enums;
using Merken.Core.Extensions;
using Merken.Core.Models.Data;
using Merken.Core.Models.Fsrs;
using Merken.Core.Services.Abstractions;

namespace Merken.Core.Services;

public class FsrsService : IFsrsService
{
    #region Members

    private readonly FsrsSettings _settings;

    #endregion

    #region Constructors

    public FsrsService(FsrsSettings? settings = null)
    {
        _settings = settings ?? new FsrsSettings();
    }

    #endregion

    #region Public methods

    public Dictionary<Rating, SchedulingInfo> Repeat(Card card)
    {
        var now = DateTime.Now;
        card = card.DeepCopy();
        card.ElapsedDays = card.State == State.New ? 0 : Convert.ToInt32((now - card.LastReviewAt).TotalDays);

        card.LastReviewAt = now;
        card.NbRepetitions += 1;

        var s = new SchedulingCards(card);
        s.UpdateState(card.State);

        switch (card.State)
        {
            case State.New:
            {
                InitializeDifficulties(s);
                s.Again.DueAt = now.AddMinutes(1);
                s.Hard.DueAt = now.AddMinutes(5);
                s.Good.DueAt = now.AddMinutes(10);
                var easyInterval = NextInterval(s.Easy.Stability);
                s.Easy.ScheduledDays = easyInterval;
                s.Easy.DueAt = now.AddDays(easyInterval);
                break;
            }

            case State.Learning:
            case State.Relearning:
            {
                const int hardInterval = 0;
                var goodInterval = NextInterval(s.Good.Stability);
                var easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
                s.Schedule(now, hardInterval, goodInterval, easyInterval);
                break;
            }

            case State.Review:
            {
                var interval = card.ElapsedDays;
                var lastDifficulty = card.Difficulty;
                var lastStability = card.Stability;
                var retrievability = Math.Pow(1 + interval / (9 * lastStability), -1);
                NextDifficulties(s, lastDifficulty, lastStability, retrievability);

                var hardInterval = NextInterval(s.Hard.Stability);
                var goodInterval = NextInterval(s.Good.Stability);
                hardInterval = Math.Min(hardInterval, goodInterval);
                goodInterval = Math.Max(goodInterval, hardInterval + 1);

                var easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
                s.Schedule(now, hardInterval, goodInterval, easyInterval);
                break;
            }
        }

        return s.RecordLog(card, now);
    }

    #endregion

    #region Private methods

    private void InitializeDifficulties(SchedulingCards s)
    {
        s.Again.Difficulty = InitializeDifficulty(Rating.Again);
        s.Again.Stability = InitializeStability(Rating.Again);
        s.Hard.Difficulty = InitializeDifficulty(Rating.Hard);
        s.Hard.Stability = InitializeStability(Rating.Hard);
        s.Good.Difficulty = InitializeDifficulty(Rating.Good);
        s.Good.Stability = InitializeStability(Rating.Good);
        s.Easy.Difficulty = InitializeDifficulty(Rating.Easy);
        s.Easy.Stability = InitializeStability(Rating.Easy);
    }

    private void NextDifficulties(SchedulingCards s, double lastDifficulty, double lastStability, double retrievability)
    {
        s.Again.Difficulty = NextDifficulty(lastDifficulty, Rating.Again);
        s.Again.Stability = NextForgetStability(
            lastDifficulty,
            lastStability,
            retrievability
        );
        s.Hard.Difficulty = NextDifficulty(lastDifficulty, Rating.Hard);
        s.Hard.Stability = NextRecallStability(
            lastDifficulty,
            lastStability,
            retrievability,
            Rating.Hard
        );
        s.Good.Difficulty = NextDifficulty(lastDifficulty, Rating.Good);
        s.Good.Stability = NextRecallStability(
            lastDifficulty,
            lastStability,
            retrievability,
            Rating.Good
        );
        s.Easy.Difficulty = NextDifficulty(lastDifficulty, Rating.Easy);
        s.Easy.Stability = NextRecallStability(
            lastDifficulty,
            lastStability,
            retrievability,
            Rating.Easy
        );
    }

    private double InitializeStability(Rating r)
    {
        return Math.Max(_settings.W[(int)r - 1], 0.1);
    }

    private double InitializeDifficulty(Rating r)
    {
        return Math.Min(Math.Max(_settings.W[4] - _settings.W[5] * ((int)r - 3), 1), 10);
    }

    private int NextInterval(double s)
    {
        var interval = s * 9 * (1 / _settings.RequestRetention - 1);
        return Math.Min(Math.Max((int)Math.Round(interval), 1), _settings.MaximumInterval);
    }

    private double NextDifficulty(double d, Rating r)
    {
        var nextDifficulty = d - _settings.W[6] * ((int)r - 3);
        return Math.Min(Math.Max(MeanReversion(_settings.W[4], nextDifficulty), 1), 10);
    }

    private double MeanReversion(double init, double current)
    {
        return _settings.W[7] * init + (1 - _settings.W[7]) * current;
    }

    private double NextRecallStability(double d, double s, double r, Rating rating)
    {
        var hardPenalty = rating == Rating.Hard ? _settings.W[15] : 1;
        var easyBonus = rating == Rating.Easy ? _settings.W[16] : 1;
        return (
            s * (1 + Math.Exp(_settings.W[8]) * (11 - d) * Math.Pow(s, -_settings.W[9]) *
                (Math.Exp((1 - r) * _settings.W[10]) - 1) * hardPenalty * easyBonus)
        );
    }

    private double NextForgetStability(double d, double s, double r)
    {
        return _settings.W[11] * Math.Pow(d, -_settings.W[12]) * (Math.Pow(s + 1, _settings.W[13]) - 1) *
               Math.Exp((1 - r) * _settings.W[14]);
    }

    #endregion
}