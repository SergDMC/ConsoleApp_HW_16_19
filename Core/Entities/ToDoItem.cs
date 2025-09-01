using System;

using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Core.Entities
{
    public class ToDoItem
    {
        public int Id { get; set; }                
        public int UserId { get; set; }           
        public int ListId { get; set; }            
        public string Title { get; set; } = null!;
        public string? Description { get; set; }  
        public int State { get; set; }             
        public DateTime? Deadline { get; set; }    
        public DateTime CreatedAt { get; set; }

        public ToDoUser? User { get; set; }
        public ToDoList? List { get; set; }
    }
}
