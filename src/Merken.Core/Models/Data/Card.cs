using System.Text.Json.Serialization;
using Merken.Core.Enums;

namespace Merken.Core.Models.Data;

public class Card
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("dueAt")]
    public DateTime DueAt { get; set; } = DateTime.Now;

    [JsonPropertyName("stability")]
    public double Stability { get; set; }

    [JsonPropertyName("difficulty")]
    public double Difficulty { get; set; }

    [JsonPropertyName("elapsedDays")]
    public int ElapsedDays { get; set; }

    [JsonPropertyName("scheduledDays")]
    public int ScheduledDays { get; set; }

    [JsonPropertyName("nbRepetitions")]
    public int NbRepetitions { get; set; }

    [JsonPropertyName("nbMistakes")]
    public int NbMistakes { get; set; }

    [JsonPropertyName("state")]
    public State State { get; set; } = State.New;

    [JsonPropertyName("lastReviewAt")]
    public DateTime LastReviewAt { get; set; } = DateTime.Now;
}