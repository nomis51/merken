using System.Runtime.InteropServices;
using Merken.Core.Models.Abstractions;
using Merken.Core.Models.Terminal;
using Merken.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Merken.Core.Services;

public class GitService : IGitService
{
    #region Constants

    private const string GitProcessName = "git";

    #endregion

    #region Members

    private readonly ILogger<GitService> _logger;

    #endregion

    #region Constructors

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Public methods

    public async Task<bool> Clone(string url)
    {
        var tmpPath = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Directory.Move() fails on linux, because it doesn't allow moving files from different devices (aka /tmp -> /home)
            tmpPath = Path.Join(Environment
                .GetFolderPath(Environment.SpecialFolder.UserProfile), ".tmp");
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            tmpPath = Path.GetTempPath();
        }

        var dirName = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(dirName)) return false;

        var dirPath = Path.Join(tmpPath, dirName);
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }

        try
        {
            var result = await new TerminalSession()
                .Command([
                    GitProcessName,
                    "clone",
                    url,
                    dirPath
                ])
                .Execute();

            // For some reason, git clone output to stderr even though there is no error
            if (!result.ErrorLines.FirstOrDefault()?.Contains("Cloning into") ?? true)
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to verify git installation");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
        }

        if (!Directory.Exists(dirPath)) return false;

        var appDataLocation = StorageService<DbModel>.DataFolder;
        var sourcePath = dirPath.TrimEnd('\\', ' ');
        var targetPath = appDataLocation.TrimEnd('\\', ' ');
        var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
            .GroupBy(Path.GetDirectoryName);
        foreach (var folder in files)
        {
            var targetFolder = folder.Key!.Replace(sourcePath, targetPath);
            Directory.CreateDirectory(targetFolder);
            foreach (var file in folder)
            {
                var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Move(file, targetFile);
            }
        }

        Directory.Delete(dirPath, true);

        if (!Directory.Exists(Path.Join(appDataLocation, ".git"))) return false;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists(tmpPath))
        {
            Directory.Delete(tmpPath);
        }

        return true;
    }

    public async Task<bool> Add(string filePath)
    {
        try
        {
            var result = await new TerminalSession(StorageService<DbModel>.DataFolder)
                .Command([
                    GitProcessName,
                    "add",
                    "--all",
                    "--",
                    ":!.lock"
                ])
                .Execute();
            if (!result.Successful && result.ErrorLines.Count != 0 &
                result.ErrorLines.FirstOrDefault(l =>
                    l.Contains("The following paths are ignored by one of your .gitignore files:")) is null)
            {
                throw new Exception(string.Join("\n", result.ErrorLines));
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while performing git add");
            return false;
        }
    }

    public async Task<bool> Commit(string message)
    {
        try
        {
            var result = await new TerminalSession(StorageService<DbModel>.DataFolder)
                .Command([
                    GitProcessName,
                    "commit",
                    "-m",
                    $"\"{message}\""
                ])
                .Execute();
            if (!result.Successful &&
                result.OutputLines.FirstOrDefault(l => l.Contains("nothing to commit, working tree clean")) is null)
            {
                throw new Exception(string.Join("\n", result.ErrorLines));
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while performing git commit");
            return false;
        }
    }

    public async Task<bool> Push()
    {
        try
        {
            var result = await new TerminalSession(StorageService<DbModel>.DataFolder)
                .Command([
                    GitProcessName,
                    "push"
                ])
                .Execute();
            return result.Successful;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while performing git push");
            return false;
        }
    }

    public async Task<bool> Pull()
    {
        try
        {
            var result = await new TerminalSession(StorageService<DbModel>.DataFolder)
                .Command([
                    GitProcessName,
                    "pull"
                ])
                .Execute();
            return result.Successful;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while performing git pull");
            return false;
        }
    }

    #endregion
}