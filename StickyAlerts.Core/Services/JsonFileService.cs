using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StickyAlerts.Core.Services
{
    public interface IJsonFileService<T> where T : class
    {
        event EventHandler<FileChangedEventArgs<T>> FileChanged;

        Task SaveAsync(string filePath, T data);
        Task<T> LoadAsync(string filePath);
        void WatchFile(string filePath);
        void StopWatchingFile(string filePath);
    }

    public class JsonFileService<T> : IJsonFileService<T> where T : class
    {
        private readonly ConcurrentDictionary<string, FileSystemWatcher> watchers = new ConcurrentDictionary<string, FileSystemWatcher>();

        public event EventHandler<FileChangedEventArgs<T>>? FileChanged;

        public async Task SaveAsync(string filePath, T data)
        {
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<T> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                await SaveAsync(filePath, default(T));
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }

        public void WatchFile(string filePath)
        {
            if (watchers.ContainsKey(filePath))
                return;

            var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            watcher.Changed += async (sender, args) => await OnFileChangedAsync(filePath);
            watcher.Deleted += async (sender, args) => await OnFileChangedAsync(filePath);
            watcher.Renamed += async (sender, args) => await OnFileChangedAsync(filePath);

            watcher.EnableRaisingEvents = true;
            watchers[filePath] = watcher;
        }

        public void StopWatchingFile(string filePath)
        {
            if (watchers.TryRemove(filePath, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }

        private async Task OnFileChangedAsync(string filePath)
        {
            try
            {
                var data = await LoadAsync(filePath);
                FileChanged?.Invoke(this, new FileChangedEventArgs<T>(filePath, data));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling file change: {ex.Message}");
            }
        }
    }

    public class FileChangedEventArgs<T> : EventArgs
    {
        public string FilePath { get; }
        public T Data { get; }

        public FileChangedEventArgs(string filePath, T data)
        {
            FilePath = filePath;
            Data = data;
        }
    }
}
