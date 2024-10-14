namespace Merken.Models;

public record Keybind(
    string Key,
    string Label,
    string? LabelColor = null
);