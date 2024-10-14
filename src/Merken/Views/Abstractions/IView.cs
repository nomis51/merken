using Merken.Models;

namespace Merken.Views.Abstractions;

public interface IView
{
    Task<ViewResult> Render(object? args = null);
}