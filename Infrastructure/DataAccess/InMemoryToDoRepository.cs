using Core.DataAccess;
using Core.Entities;

namespace Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public void Add(ToDoItem item) => _items.Add(item);

        public void Update(ToDoItem item)
        {
            var index = _items.FindIndex(i => i.Id == item.Id);
            if (index >= 0) _items[index] = item;
        }

        public void Delete(Guid id) =>
            _items.RemoveAll(i => i.Id == id);

        public ToDoItem? Get(Guid id) =>
            _items.FirstOrDefault(i => i.Id == id);

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) =>
            _items.Where(i => i.UserId == userId).ToList();

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) =>
            _items.Where(i => i.UserId == userId && i.IsActive).ToList();

        public bool ExistsByName(Guid userId, string name) =>
            _items.Any(i => i.UserId == userId && i.Name == name);

        public int CountActive(Guid userId) =>
            _items.Count(i => i.UserId == userId && i.IsActive);

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate) =>
            _items.Where(i => i.UserId == userId).Where(predicate).ToList();
    }
}
