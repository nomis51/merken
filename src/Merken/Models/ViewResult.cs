namespace Merken.Models;

public record ViewResult(
    Type? ViewType,
    object? Args = null
);