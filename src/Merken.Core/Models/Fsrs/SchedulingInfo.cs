using Merken.Core.Models.Data;

namespace Merken.Core.Models.Fsrs;

public class SchedulingInfo
{
    public Card Card { get; set; }
    public ReviewLog ReviewLog { get; set; }

    public SchedulingInfo(Card card, ReviewLog reviewLog)
    {
        Card = card;
        ReviewLog = reviewLog;
    }
}