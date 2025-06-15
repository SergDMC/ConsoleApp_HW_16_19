using Otus.ToDoList.ConsoleBot.Types;
using System.Threading.Tasks;
using System.Threading;

using ToDoListConsoleBot.Models;

public interface IUserService
{
    Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
    Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
}
