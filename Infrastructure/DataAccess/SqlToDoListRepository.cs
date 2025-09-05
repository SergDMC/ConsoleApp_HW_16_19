using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToDoListConsoleBot.Core.DataAccess;

using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.GetTable<ToDoListModel>()
                .LoadWith(l => l.User)
                .LoadWith(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            return model is null ? null : ModelMapper.MapFromModel(model);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.GetTable<ToDoListModel>()
                .LoadWith(l => l.User)
                .LoadWith(l => l.Items)
                .Where(l => l.UserId == userId)
                .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(list);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.GetTable<ToDoListModel>()
                .Where(l => l.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.GetTable<ToDoListModel>()
                .AnyAsync(l => l.UserId == userId && l.Name == name, ct);
        }
    }
}
