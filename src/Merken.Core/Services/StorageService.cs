using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Merken.Core.Models.Abstractions;
using Merken.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Merken.Core.Services;

public class StorageService<T> : IStorageService<T>
    where T : DbModel
{
    #region Constants

    private const string DataFolderName = "data";

    #endregion

    #region Members

    private readonly ILogger<StorageService<T>> _logger;
    private readonly IGitService _gitService;

    #endregion

    #region Props

    public static string DataFolder => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        $".{nameof(Merken).ToLower()}",
        DataFolderName
    );

    private string FolderName => typeof(T).GetCustomAttribute<DescriptionAttribute>()!.Description;

    private static bool IsGitInitialized => Directory.Exists(
        Path.Join(DataFolder, ".git")
    );

    #endregion

    #region Constructors

    public StorageService(ILogger<StorageService<T>> logger, IGitService gitService)
    {
        _logger = logger;
        _gitService = gitService;
    }

    #endregion

    #region Public methods

    public Task<T?> GetByIdAsync(Guid id)
    {
        return Read(id);
    }

    public Task<List<T>> GetAllAsync()
    {
        return ReadAll();
    }

    public async Task<T?> AddAsync(T model)
    {
        if (await ExistsAsync(model.Id)) return null;
        if (!await Write(model)) return null;

        return model;
    }

    public async Task<T?> UpdateAsync(T model)
    {
        if (!await ExistsAsync(model.Id)) return null;
        if (!await Write(model)) return null;

        return model;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await ExistsAsync(id)) return false;

        var filePath = Path.Join(DataFolder, FolderName, $"{id}.json");
        try
        {
            File.Delete(filePath);

            if (IsGitInitialized)
            {
                _ = Task.Run(Commit);
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to delete {FilePath}: {Exception}", filePath, e);
            return false;
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        var filePath = Path.Join(DataFolder, FolderName, $"{id}.json");
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<bool> Sync()
    {
        if (!IsGitInitialized) return true;
        if (!await _gitService.Pull()) return false;
        return await _gitService.Push();
    }

    #endregion

    #region Private methods

    private async Task<bool> Write(T model)
    {
        var filePath = Path.Join(DataFolder, FolderName, $"{model.Id}.json");
        EnsureFolderExists(filePath);

        try
        {
            var json = JsonSerializer.Serialize(model);
            await File.WriteAllTextAsync(filePath, json);

            if (IsGitInitialized)
            {
                _ = Task.Run(Commit);
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to write {FilePath}", filePath);
            return false;
        }
    }

    private async Task<T?> Read(Guid id)
    {
        var filePath = Path.Join(DataFolder, FolderName, $"{id}.json");
        EnsureFolderExists(filePath);

        if (!File.Exists(filePath)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json)!;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to read {FilePath}", filePath);
            return null;
        }
    }

    private async Task<List<T>> ReadAll()
    {
        var folder = Path.Join(DataFolder, FolderName);
        EnsureFolderExists(folder);

        if (!Directory.Exists(folder)) return [];

        var files = Directory.EnumerateFiles(folder, "*.json");
        var data = new List<T>();
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                data.Add(JsonSerializer.Deserialize<T>(json)!);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to read {FilePath}", file);
            }
        }

        return data;
    }

    private void EnsureFolderExists(string path)
    {
        if (Directory.Exists(path)) return;

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private async Task Commit()
    {
        if (!await _gitService.Add("."))
        {
            _logger.LogWarning("Unable to add files to git");
            return;
        }

        if (!await _gitService.Commit("Update deck"))
        {
            _logger.LogWarning("Unable to commit changes to git");
            return;
        }
    }

    #endregion
}