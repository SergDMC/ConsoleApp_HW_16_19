using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using ToDoListConsoleBot.Models;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ToDoItem item, CancellationToken cancellationToken);
    Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken);
    Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken ct);

}
