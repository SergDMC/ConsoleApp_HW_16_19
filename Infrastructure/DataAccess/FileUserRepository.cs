using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;

        public FileUserRepository(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            var filePath = Path.Combine(_basePath, $"{user.UserId}.json");
            var json = JsonSerializer.Serialize(user);
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task<ToDoUser?> GetByIdAsync(Guid userId, CancellationToken ct)
        {
            var filePath = Path.Combine(_basePath, $"{userId}.json");
            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<ToDoUser>(json);
        }

        public async Task<ToDoUser?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            var users = await Task.WhenAll(
                Directory.GetFiles(_basePath, "*.json")
                    .Select(async file =>
                    {
                        var json = await File.ReadAllTextAsync(file, ct);
                        return JsonSerializer.Deserialize<ToDoUser>(json);
                    })
            );

            return users.FirstOrDefault(u => u?.TelegramUserId == telegramUserId);
        }

        public async Task<bool> ExistsAsync(long telegramUserId, CancellationToken ct)
        {
            var user = await GetByTelegramUserIdAsync(telegramUserId, ct);
            return user != null;
        }
    }
}
