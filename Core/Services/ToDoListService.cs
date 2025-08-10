using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToDoListConsoleBot.Core.DataAccess;
using ToDoListConsoleBot.Core.Entities;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _repo;

        public ToDoListService(IToDoListRepository repo)
        {
            _repo = repo;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (name.Length > 10)
                throw new ArgumentException("Название списка не может быть больше 10 символов");

            if (await _repo.ExistsByName(user.UserId, name, ct))
                throw new InvalidOperationException("Список с таким именем уже существует");

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.Add(list, ct);
            return list;
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct) =>
            _repo.Get(id, ct);

        public Task Delete(Guid id, CancellationToken ct) =>
            _repo.Delete(id, ct);

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct) =>
            _repo.GetByUserId(userId, ct);
    }
}
