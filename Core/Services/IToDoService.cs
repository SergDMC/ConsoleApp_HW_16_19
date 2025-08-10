using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using ToDoListConsoleBot.Models;
using ToDoListConsoleBot.Core.Entities;

public interface IToDoService
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ToDoItem> AddAsync(ToDoUser user, string name, ToDoList? list, DateTime deadline, CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
}
