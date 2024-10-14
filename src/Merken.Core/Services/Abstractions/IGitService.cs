namespace Merken.Core.Services.Abstractions;

public interface IGitService
{
    Task<bool> Clone(string url);
    Task<bool> Add(string filePath);
    Task<bool> Commit(string message);
    Task<bool> Push();
    Task<bool> Pull();
}