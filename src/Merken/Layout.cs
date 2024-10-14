using Merken.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Merken;

public class Layout
{
    #region Public methods

    public void Render(IRenderable renderable, Keybind[] keybinds, int offset = 0)
    {
        var table = new Table()
            .Expand()
            .Centered()
            .AddColumn(string.Empty)
            .HideHeaders()
            .HideRowSeparators()
            .NoBorder();

        table.AddRow(renderable);

        for (var i = 0; i < Console.BufferHeight - 6 - offset; ++i)
        {
            table.AddEmptyRow();
        }

        table.AddRow(
            new Panel(
                    new Markup(
                            string.Join(
                                " • ",
                                keybinds.Select(k => $"[white]{k.Key}[/] [{k.LabelColor ?? "gray"}]{k.Label}[/]")
                            )
                        )
                        .Centered()
                )
                .Expand()
                .NoBorder()
        );

        AnsiConsole.Write(table);
    }

    #endregion
}