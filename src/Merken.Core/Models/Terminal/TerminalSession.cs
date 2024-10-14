using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;

namespace Merken.Core.Models.Terminal;

public class TerminalSession
{
    #region Members

    private readonly Process _process = new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        },
    };

    private readonly List<List<string>> _arguments = [];
    private readonly bool _waitForExit;

    #endregion

    #region Constructors

    public TerminalSession(string workingDirectory = ".", bool waitForExit = true)
    {
        _process.StartInfo.WorkingDirectory = workingDirectory;
        _process.StartInfo.RedirectStandardError = waitForExit;
        _process.StartInfo.RedirectStandardOutput = waitForExit;
        _waitForExit = waitForExit;
    }

    #endregion

    #region Public methods

    public TerminalSession Command(IEnumerable<string> arguments)
    {
        _arguments.Add(arguments.ToList());
        return this;
    }

    public async Task<TerminalSessionResult> Execute()
    {
        var pipedCommands = string.Join(" | ", _arguments.Select(args => string.Join(" ", args)));
        try
        {
            _process.StartInfo.Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"/C {pipedCommands}"
                : $"-c '{pipedCommands}'";
            _process.Start();
            if (!_waitForExit) return new TerminalSessionResult();

            await _process.WaitForExitAsync();
            var stderr = await _process.StandardError.ReadToEndAsync();
            var stdout = await _process.StandardOutput.ReadToEndAsync();

            return new TerminalSessionResult(
                _process.ExitCode == 0,
                stderr.Split(Environment.NewLine,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                stdout.Split(Environment.NewLine,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            );
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to execute process '{Command} {Args}'", _process.StartInfo.FileName,
                pipedCommands);
        }

        return new TerminalSessionResult();
    }

    #endregion
}