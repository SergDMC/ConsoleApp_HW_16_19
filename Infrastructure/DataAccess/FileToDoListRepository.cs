using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ToDoListConsoleBot.Core.DataAccess;
using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _basePath;

        public FileToDoListRepository(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        private string GetFilePath(Guid id) => Path.Combine(_basePath, $"{id}.json");

        public async Task AddAsync(ToDoList list, CancellationToken ct)
        {
            var filePath = GetFilePath(list.Id);
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }

        public async Task<ToDoList?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<ToDoList>(json);
        }

        public async Task<IEnumerable<ToDoList>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var files = Directory.GetFiles(_basePath, "*.json");

            var lists = await Task.WhenAll(
                files.Select(async file =>
                {
                    var json = await File.ReadAllTextAsync(file, ct);
                    var list = JsonSerializer.Deserialize<ToDoList>(json);
                    return list;
                })
            );

            return lists.Where(l => l != null && l.UserId == userId)!;
        }

        public async Task UpdateAsync(ToDoList list, CancellationToken ct)
        {
            var filePath = GetFilePath(list.Id);
            if (!File.Exists(filePath))
                throw new InvalidOperationException("Список не найден для обновления");

            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
        }
    }
}
