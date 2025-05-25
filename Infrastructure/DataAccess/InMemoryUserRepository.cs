using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public void Add(ToDoUser user) => _users.Add(user);

        public ToDoUser? GetUser(Guid userId) =>
            _users.FirstOrDefault(u => u.Id == userId);

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId) =>
            _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }
}
