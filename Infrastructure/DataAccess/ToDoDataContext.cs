using LinqToDB.Data;
using LinqToDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.DataAccess;
using ToDoListConsoleBot.Infrastructure.DataAccess.Models;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString)
            : base(ProviderName.PostgreSQL, connectionString) { }

        public ITable<ToDoUserModel> ToDoUsers => GetTable<ToDoUserModel>();

        public ITable<ToDoListModel> ToDoLists => GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItems => GetTable<ToDoItemModel>();
        public ITable<NotificationModel> Notifications => GetTable<NotificationModel>();

    }

}
