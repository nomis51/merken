using System.Text.Json.Serialization;

namespace Merken.Core.Models.Abstractions;

public class DbModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}