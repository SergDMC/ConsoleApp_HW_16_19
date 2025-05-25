using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Services
{
    public class UserService : IUserService
    {
        private readonly Dictionary<long, ToDoUser> _users = new();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            if (_users.ContainsKey(telegramUserId))
                return _users[telegramUserId];

            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };

            _users[telegramUserId] = user;
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            _users.TryGetValue(telegramUserId, out var user);
            return user;
        }
    }
}
