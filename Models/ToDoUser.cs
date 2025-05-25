namespace ToDoListConsoleBot.Models
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}
