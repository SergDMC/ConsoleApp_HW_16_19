using Core.DataAccess;
using Core.Repositories;
using Core.Services;

using Infrastructure.DataAccess;

using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Threading.Tasks;

using System.Threading;

using System;

using ToDoListConsoleBot.Bot;
using ToDoListConsoleBot.Services;

class Program
{
    static async Task Main(string[] args)
    {
        // Создаем репозитории
        IUserRepository userRepository = new InMemoryUserRepository();
        IToDoRepository toDoRepository = new InMemoryToDoRepository();

        // Создаем сервисы
        IUserService userService = new UserService(userRepository);
        IToDoService toDoService = new ToDoService(toDoRepository);
        IToDoReportService reportService = new ToDoReportService(toDoRepository);

        // Создаем клиента бота и обработчик обновлений
        ITelegramBotClient botClient = new ConsoleBotClient();
        var updateHandler = new UpdateHandler(botClient, userService, toDoService, reportService);

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
                var cts = new CancellationTokenSource();
                await updateHandler.HandleUpdateAsync(update, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
