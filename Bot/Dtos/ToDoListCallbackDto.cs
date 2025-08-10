using System;

namespace ToDoListConsoleBot.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Callback data is empty", nameof(input));

            var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);
            Guid? parsedId = null;

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var guidValue))
            {
                parsedId = guidValue;
            }

            return new ToDoListCallbackDto
            {
                Action = parts.Length > 0 ? parts[0] : string.Empty,
                ToDoListId = parsedId
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
