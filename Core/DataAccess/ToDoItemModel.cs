using LinqToDB.Mapping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Core.DataAccess
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey, Identity] public int Id { get; set; }                 
        [Column] public int UserId { get; set; }                           
        [Column] public int ListId { get; set; }                           
        [Column] public string Title { get; set; } = null!;                
        [Column] public string? Description { get; set; }                  
        [Column] public int State { get; set; }                            
        [Column] public DateTime? Deadline { get; set; }                   
        [Column] public DateTime CreatedAt { get; set; }                   

        // Навигация
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel? User { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? List { get; set; }
    }
}
