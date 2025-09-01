using System;
using System.Collections.Generic;
using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Core.Entities
{

    public class ToDoUser
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public List<ToDoList> Lists { get; set; } = new();
        public List<ToDoItem> Items { get; set; } = new();
    }

}