using System;

namespace ToDoListConsoleBot.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; } = string.Empty;

        public static CallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Callback data is empty", nameof(input));

            var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);
            return new CallbackDto
            {
                Action = parts.Length > 0 ? parts[0] : input
            };
        }

        public override string ToString()
        {
            return Action;
        }
    }
}
