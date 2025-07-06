using System;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Core.Services;
using Infrastructure.DataAccess;
using ToDoListConsoleBot.Bot;
using ToDoListConsoleBot.Services;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Репозитории
        IUserRepository userRepository = new InMemoryUserRepository();
        IToDoRepository toDoRepository = new InMemoryToDoRepository();

        // Сервисы
        IUserService userService = new UserService(userRepository);
        IToDoService toDoService = new ToDoService(toDoRepository);
        IToDoReportService reportService = new ToDoReportService(toDoRepository);

        // Токен из переменной среды
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: переменная среды TELEGRAM_BOT_TOKEN не установлена.");
            Console.ResetColor();
            return;
        }

        ITelegramBotClient botClient = new TelegramBotClient(token);
        var updateHandler = new UpdateHandler(botClient, userService, toDoService, reportService);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message },
            DropPendingUpdates = true
        };

        botClient.StartReceiving(
            updateHandler.HandleUpdateAsync,
            updateHandler.HandlePollingErrorAsync,
            receiverOptions,
            cancellationToken
        );

        var me = await botClient.GetMeAsync(cancellationToken);
        Console.WriteLine($"{me.FirstName} (@{me.Username}) запущен!");

        // Установка списка доступных команд в меню Telegram
        await botClient.SetMyCommandsAsync(new[]
        {
            new BotCommand { Command = "start", Description = "Регистрация" },
            new BotCommand { Command = "addtask", Description = "Добавить задачу" },
            new BotCommand { Command = "removetask", Description = "Удалить задачу" },
            new BotCommand { Command = "completetask", Description = "Завершить задачу" },
            new BotCommand { Command = "showtasks", Description = "Показать активные задачи" },
            new BotCommand { Command = "showalltasks", Description = "Показать все задачи" },
            new BotCommand { Command = "report", Description = "Статистика задач" },
            new BotCommand { Command = "find", Description = "Найти задачу по имени" },
            new BotCommand { Command = "help", Description = "Справка" },
            new BotCommand { Command = "info", Description = "О боте" }
        }, cancellationToken: cancellationToken);

        Console.WriteLine("Нажмите клавишу A для выхода.");

        // Обработка клавиш
        while (!cancellationToken.IsCancellationRequested)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.A)
            {
                Console.WriteLine("\nЗавершение работы...");
                cts.Cancel();
                break;
            }
            else
            {
                Console.WriteLine($"Бот: {me.FirstName}, username: @{me.Username}, ID: {me.Id}");
            }
        }

        // Ждем завершения фоновых задач, если потребуется
        await Task.Delay(1000);
    }
}
