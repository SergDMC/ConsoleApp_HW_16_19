using LinqToDB.Mapping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Core.DataAccess
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey, Identity] public int Id { get; set; }              
        [Column] public int UserId { get; set; }                         
        [Column] public string Title { get; set; } = null!;              
        [Column] public DateTime CreatedAt { get; set; }                 

        // Навигация
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel? User { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoItemModel.ListId))]
        public IEnumerable<ToDoItemModel>? Items { get; set; }
    }
}
