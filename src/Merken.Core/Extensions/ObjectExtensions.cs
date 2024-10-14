using System.Text.Json;

namespace Merken.Core.Extensions;

public static class ObjectExtensions
{
    #region Public methods

    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonSerializer.Serialize(self);
        return JsonSerializer.Deserialize<T>(serialized)!;
    }

    #endregion
}