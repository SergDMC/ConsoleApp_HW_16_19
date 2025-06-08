using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Services
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();
        private const int MaxTasksPerUser = 10;
        private const int MaxTaskNameLength = 100;

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя задачи не может быть пустым.");

            if (name.Length > MaxTaskNameLength)
                throw new ArgumentException($"Имя задачи не может превышать {MaxTaskNameLength} символов.");

            var userTasks = _items.Where(i => i.User.UserId == user.UserId).ToList();

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

            _items.Add(item);
            return item;
        }

        public void Delete(Guid id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
                _items.Remove(item);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _items.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _items.Where(i => i.User.UserId == userId).ToList();
        }

        public void MarkCompleted(Guid id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null && item.State == ToDoItemState.Active)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.Now;
            }
        }
        
        private readonly IToDoRepository _toDoRepository;

        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.Id, x => x.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        }


    }
}
