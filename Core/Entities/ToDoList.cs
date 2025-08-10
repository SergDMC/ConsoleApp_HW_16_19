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
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ToDoUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
