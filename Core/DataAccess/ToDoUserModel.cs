using LinqToDB.Mapping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Core.DataAccess
{

    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey, Identity] public int Id { get; set; }
        [Column] public long TelegramUserId { get; set; }
        [Column] public string UserName { get; set; } = null!;
        [Column] public DateTime CreatedAt { get; set; }

        // Навигация
        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoListModel.UserId))]
        public IEnumerable<ToDoListModel>? Lists { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoItemModel.UserId))]
        public IEnumerable<ToDoItemModel>? Items { get; set; }
    }
}
