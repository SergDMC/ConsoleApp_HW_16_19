using Core.DataAccess;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private const int MaxTasksPerUser = 10;
        private const int MaxTaskNameLength = 100;

        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя задачи не может быть пустым.");

            if (name.Length > MaxTaskNameLength)
                throw new ArgumentException($"Имя задачи не может превышать {MaxTaskNameLength} символов.");

            var userTasks = await _toDoRepository.GetAllByUserIdAsync(user.UserId, cancellationToken);

            if (userTasks.Count >= MaxTasksPerUser)
                throw new InvalidOperationException($"Максимальное количество задач: {MaxTasksPerUser}.");

            if (userTasks.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Задача с таким именем уже существует.");

            var item = new ToDoItem
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.Now,
                State = ToDoItemState.Active
            };

            await _toDoRepository.AddAsync(item, cancellationToken);
            return item;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
        }

        public async Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = await _toDoRepository.GetAsync(id, cancellationToken);
            if (item != null && item.State == ToDoItemState.Active)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.Now;
                await _toDoRepository.UpdateAsync(item, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            return await _toDoRepository.FindAsync(user.UserId, x =>
                x.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }
    }
}
