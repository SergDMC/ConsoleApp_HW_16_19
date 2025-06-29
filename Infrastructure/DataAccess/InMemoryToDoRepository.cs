using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToDoListConsoleBot.Models; 

namespace Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            var index = _items.FindIndex(i => i.Id == item.Id);
            if (index >= 0) _items[index] = item;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            _items.RemoveAll(i => i.Id == id);
            return Task.CompletedTask;
        }

        public Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var result = _items.Where(i => i.UserId == userId).ToList();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var result = _items.Where(i => i.UserId == userId && i.IsActive).ToList();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            var exists = _items.Any(i => i.UserId == userId && i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            var count = _items.Count(i => i.UserId == userId && i.IsActive);
            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var result = _items
                .Where(i => i.UserId == userId)
                .Where(predicate)
                .ToList();

            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }
    }
}
