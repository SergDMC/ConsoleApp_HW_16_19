using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToDoListConsoleBot.Core.Entities;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Core.Services
{
    public interface IToDoListService
    {
        Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct);
        Task<ToDoList?> Get(Guid id, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct);
    }
}
