using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.TelegramBot.Dto;

namespace ToDoListConsoleBot.Bot.Dtos
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }

        public static new PagedListCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            return new PagedListCallbackDto
            {
                Action = parts[0],
                ToDoListId = string.IsNullOrEmpty(parts[1]) ? null : Guid.Parse(parts[1]),
                Page = int.Parse(parts[2])
            };
        }
    }
}
