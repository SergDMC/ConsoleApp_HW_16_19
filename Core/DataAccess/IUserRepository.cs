using System.Threading.Tasks;
using System.Threading;
using System;
using ToDoListConsoleBot.Models;
using System.Collections.Generic;

public interface IUserRepository
{
    Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken);
    Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct);

}
