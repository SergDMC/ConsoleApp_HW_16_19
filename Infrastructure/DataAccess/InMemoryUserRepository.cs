using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Models;

namespace Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            return Task.FromResult(user);
        }

        public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            var user = _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
            return Task.FromResult(user);
        }
    }
}
