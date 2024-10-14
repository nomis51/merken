using Merken.Core.Enums;

namespace Merken.Core.Models.Fsrs;

public class ReviewLog
{
    public Rating Rating { get; set; }
    public int ScheduledDays { get; set; }
    public int ElapsedDays { get; set; }
    public DateTime Review { get; set; }
    public State State { get; set; }

    // ReSharper disable once UnusedMember.Global
    public ReviewLog()
    {
    }

    public ReviewLog(Rating rating, int scheduledDays, int elapsedDays, DateTime review, State state)
    {
        Rating = rating;
        ScheduledDays = scheduledDays;
        ElapsedDays = elapsedDays;
        Review = review;
        State = state;
    }
}