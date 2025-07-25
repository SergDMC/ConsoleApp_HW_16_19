﻿using System;

namespace ToDoListConsoleBot.Models
{
    public class ToDoItem
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }

        // Добавленные свойства для удобства репозитория
        public Guid UserId => User?.UserId ?? Guid.Empty;

        public bool IsActive => State == ToDoItemState.Active;
    }
}
