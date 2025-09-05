using System;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Core.Services;
using ToDoListConsoleBot.Bot;
using ToDoListConsoleBot.Services;
using ToDoListConsoleBot.Infrastructure.DataAccess;
using ToDoListConsoleBot.Core.DataAccess;


internal class Program
{
    static async Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: переменная среды TELEGRAM_BOT_TOKEN не установлена.");
            Console.ResetColor();
            return;
        }

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Получаем IConfiguration
                IConfiguration configuration = context.Configuration;
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                // SQL-репозитории
                services.AddSingleton<IDataContextFactory<ToDoDataContext>>(
                    _ => new DataContextFactory(connectionString)
                );

                services.AddScoped<IUserRepository, SqlUserRepository>();
                services.AddScoped<IToDoRepository, SqlToDoRepository>();
                services.AddScoped<IToDoListRepository, SqlToDoListRepository>();

                // Сервисы
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IToDoService, ToDoService>();
                services.AddScoped<IToDoReportService, ToDoReportService>();

                // TelegramBotClient и UpdateHandler
                services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));
                services.AddScoped<UpdateHandler>();
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var botClient = services.GetRequiredService<ITelegramBotClient>();
        var updateHandler = services.GetRequiredService<UpdateHandler>();

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

        await botClient.SetMyCommandsAsync(new[]
        {
            new BotCommand { Command = "start", Description = "Регистрация" },
            new BotCommand { Command = "addtask", Description = "Добавить задачу" },
            new BotCommand { Command = "showtasks", Description = "Показать активные задачи" },
            new BotCommand { Command = "showalltasks", Description = "Показать все задачи" },
            new BotCommand { Command = "report", Description = "Статистика задач" },
            new BotCommand { Command = "find", Description = "Найти задачу по имени" },
            new BotCommand { Command = "help", Description = "Справка" },
            new BotCommand { Command = "info", Description = "О боте" }
        }, cancellationToken: cancellationToken);

        Console.WriteLine("Нажмите клавишу A для выхода.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.A)
            {
                Console.WriteLine("\nЗавершение работы...");
                cts.Cancel();
                break;
            }
        }

        await host.RunAsync();
    }
}
