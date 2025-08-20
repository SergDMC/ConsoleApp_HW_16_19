using System.Text.Json;
using Core.Entities;
using Core.Services;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _basePath;
        private readonly string _indexFile;
        private readonly ConcurrentDictionary<Guid, Guid> _index = new();

        public FileToDoRepository(string basePath)
        {
            _basePath = basePath;
            _indexFile = Path.Combine(_basePath, "index.json");

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            LoadIndex();
        }

        private void LoadIndex()
        {
            if (File.Exists(_indexFile))
            {
                var json = File.ReadAllText(_indexFile);
                var dict = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json);
                if (dict != null)
                {
                    foreach (var pair in dict)
                        _index[pair.Key] = pair.Value;
                }
            }
        }

        private async Task SaveIndexAsync(CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(_index);
            await File.WriteAllTextAsync(_indexFile, json, ct);
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            var userFolder = Path.Combine(_basePath, item.User.UserId.ToString());
            Directory.CreateDirectory(userFolder);

            var filePath = Path.Combine(userFolder, $"{item.Id}.json");
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(filePath, json, ct);

            _index[item.Id] = item.User.UserId;
            await SaveIndexAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!_index.TryGetValue(id, out var userId))
                return;

            var filePath = Path.Combine(_basePath, userId.ToString(), $"{id}.json");
            if (File.Exists(filePath))
                File.Delete(filePath);

            _index.TryRemove(id, out _);
            await SaveIndexAsync(ct);
        }

        public async Task<ToDoItem?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            if (!_index.TryGetValue(id, out var userId))
                return null;

            var filePath = Path.Combine(_basePath, userId.ToString(), $"{id}.json");
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<ToDoItem>(json);
        }

        public async Task<IEnumerable<ToDoItem>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var userFolder = Path.Combine(_basePath, userId.ToString());
            if (!Directory.Exists(userFolder))
                return Enumerable.Empty<ToDoItem>();

            var files = Directory.GetFiles(userFolder, "*.json");

            var tasks = files
                .Select(async file =>
                {
                    var json = await File.ReadAllTextAsync(file, ct);
                    return JsonSerializer.Deserialize<ToDoItem>(json);
                });

            var results = await Task.WhenAll(tasks);
            return results.Where(item => item != null)!;
        }
    }
    }
}
