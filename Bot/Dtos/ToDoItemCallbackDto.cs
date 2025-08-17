using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.TelegramBot.Dto;

namespace ToDoListConsoleBot.Bot.Dtos
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        public override string ToString()
        {
            return $"{Action}|{ToDoItemId}";
        }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            return new ToDoItemCallbackDto
            {
                Action = parts[0],
                ToDoItemId = Guid.Parse(parts[1])
            };
        }
    }
}
