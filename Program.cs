using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

using ToDoListConsoleBot.Bot;
using ToDoListConsoleBot.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var userService = new UserService();
        var toDoService = new ToDoService();
        var botClient = new ConsoleBotClient();
        var updateHandler = new UpdateHandler(botClient, userService, toDoService);

        Console.WriteLine("Бот запущен. Введите команды:");

        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var update = new Update
            {
                Message = new Message
                {
                    Text = input,
                    From = new User
                    {
                        Id = 1,
                        Username = "ConsoleUser"
                    },
                    Chat = new Chat
                    {
                        Id = 1
                    }
                }
            };

            try
            {
                await updateHandler.HandleUpdateAsync(update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
