
using System.Threading.Tasks;
using System.Threading;
using System;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);
            if (existingUser != null)
                return existingUser;

            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };

            await _userRepository.AddAsync(user, cancellationToken);
            return user;
        }

        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);
        }
    }
}
