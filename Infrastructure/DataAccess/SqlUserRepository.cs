using LinqToDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(user);
            await db.InsertAsync(model, cancellationToken);
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var model = await db.ToDoUsers
                .Where(u => u.Id == (int)userId) // если в БД serial4/int4
                .FirstOrDefaultAsync(cancellationToken);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var model = await db.ToDoUsers
                .Where(u => u.TelegramUserId == telegramUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }
    }
}
