using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Core.Entities
{
    public class ToDoList
    {
        public int Id { get; set; }               
        public int UserId { get; set; }           
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public ToDoUser? User { get; set; }
        public List<ToDoItem> Items { get; set; } = new();
    }
}
