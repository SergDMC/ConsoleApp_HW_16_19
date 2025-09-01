using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.DataAccess;


namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var query = dbContext.GetTable<ToDoItemModel>()
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId);

            var models = await query.ToListAsync(cancellationToken);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var query = dbContext.GetTable<ToDoItemModel>()
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == 0); 

            var models = await query.ToListAsync(cancellationToken);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.GetTable<ToDoItemModel>()
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            return model == null ? null : ModelMapper.MapFromModel(model);
        }

        public async Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model, token: cancellationToken);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(item);
            await dbContext.UpdateAsync(model, token: cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.GetTable<ToDoItemModel>()
                .Where(i => i.Id == id)
                .DeleteAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.GetTable<ToDoItemModel>()
                .AnyAsync(i => i.UserId == userId && i.Title == name, cancellationToken);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.GetTable<ToDoItemModel>()
                .CountAsync(i => i.UserId == userId && i.State == 0, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.GetTable<ToDoItemModel>()
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(cancellationToken);

            var entities = models.Select(ModelMapper.MapFromModel).ToList();
            return entities.Where(predicate).ToList();
        }
    }
}
