using System.ComponentModel;
using System.Text.Json.Serialization;
using Merken.Core.Models.Abstractions;

namespace Merken.Core.Models.Data;

[Description("decks")]
public class Deck : DbModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cards")]
    public List<Card> Cards { get; set; } = [];
}