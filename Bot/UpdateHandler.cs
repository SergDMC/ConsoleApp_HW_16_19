using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Core.Services;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ToDoListConsoleBot.Bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService toDoService,
            IToDoReportService reportService)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _reportService = reportService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message?.Text is not { } text || update.Message.From is null)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;
            var from = message.From;

            try
            {
                var currentUser = await _userService.GetUserAsync(from.Id, cancellationToken);
                var keyboardMarkup = BuildKeyboard(currentUser != null);

                // Команды
                if (text.StartsWith("/start"))
                {
                    if (currentUser == null)
                    {
                        var newUser = await _userService.RegisterUserAsync(from.Id, from.Username ?? "", cancellationToken);
                        await _botClient.SendTextMessageAsync(chatId,
                            $"Привет, {newUser.TelegramUserName}! Вы успешно зарегистрированы.",
                            replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId,
                            "Вы уже зарегистрированы.",
                            replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                    }
                    return;
                }

                if (currentUser == null)
                {
                    await _botClient.SendTextMessageAsync(chatId,
                        "Пожалуйста, зарегистрируйтесь с помощью команды /start.",
                        replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                    return;
                }

                if (text.StartsWith("/addtask"))
                {
                    var taskName = text[8..].Trim();
                    if (string.IsNullOrEmpty(taskName))
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Пожалуйста, укажите название задачи.", cancellationToken: cancellationToken);
                        return;
                    }

                    var item = await _toDoService.AddAsync(currentUser, taskName, cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId, $"Задача добавлена: {item.Name}", cancellationToken: cancellationToken);
                }
                else if (text.StartsWith("/removetask"))
                {
                    if (Guid.TryParse(text[11..].Trim(), out var id))
                    {
                        await _toDoService.DeleteAsync(id, cancellationToken);
                        await _botClient.SendTextMessageAsync(chatId, "Задача удалена.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Неверный формат ID задачи.", cancellationToken: cancellationToken);
                    }
                }
                else if (text.StartsWith("/completetask"))
                {
                    if (Guid.TryParse(text[14..].Trim(), out var id))
                    {
                        await _toDoService.MarkCompletedAsync(id, cancellationToken);
                        await _botClient.SendTextMessageAsync(chatId, "Задача завершена.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Неверный формат ID задачи.", cancellationToken: cancellationToken);
                    }
                }
                else if (text.StartsWith("/showtasks"))
                {
                    var tasks = await _toDoService.GetActiveByUserIdAsync(currentUser.UserId, cancellationToken);
                    if (!tasks.Any())
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Нет активных задач.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            var line = $"`{task.Id}` - {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss}";
                            await _botClient.SendTextMessageAsync(chatId, line, ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (text.StartsWith("/showalltasks"))
                {
                    var tasks = await _toDoService.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
                    if (!tasks.Any())
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Нет задач.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            var line = $"`{task.Id}` - {task.Name} ({task.State}) - {task.CreatedAt:dd.MM.yyyy HH:mm:ss}";
                            await _botClient.SendTextMessageAsync(chatId, line, ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (text.StartsWith("/report"))
                {
                    var stats = await _reportService.GetUserStatsAsync(currentUser.UserId, cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId,
                        $"Статистика на {stats.generatedAt:G}\nВсего: {stats.total}, Завершено: {stats.completed}, Активных: {stats.active}.",
                        cancellationToken: cancellationToken);
                }
                else if (text.StartsWith("/find"))
                {
                    var prefix = text[5..].Trim();
                    if (string.IsNullOrEmpty(prefix))
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Используйте: /find <prefix>", cancellationToken: cancellationToken);
                        return;
                    }

                    var tasks = await _toDoService.FindAsync(currentUser, prefix, cancellationToken);
                    if (!tasks.Any())
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Задачи не найдены.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            var status = task.IsActive ? "Активна" : "Завершена";
                            await _botClient.SendTextMessageAsync(chatId, $"[{task.Id}] {task.Name} - {status}", cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (text == "/help")
                {
                    var helpText = string.Join('\n', new[]
                    {
                        "/addtask [название] – добавить задачу",
                        "/removetask [ID] – удалить задачу",
                        "/completetask [ID] – завершить задачу",
                        "/showtasks – активные задачи",
                        "/showalltasks – все задачи",
                        "/report – статистика",
                        "/find [префикс] – поиск задач",
                        "/help – справка",
                        "/info – о боте"
                    });

                    await _botClient.SendTextMessageAsync(chatId, helpText, cancellationToken: cancellationToken);
                }
                else if (text == "/info")
                {
                    await _botClient.SendTextMessageAsync(chatId,
                        "Я Telegram ToDo-бот. Помогаю управлять вашими задачами.",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId,
                        "Неизвестная команда. Используйте /help для списка.",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Polling error: {exception}");
            return Task.CompletedTask;
        }

        private static IReplyMarkup BuildKeyboard(bool isRegistered)
        {
            return isRegistered
                ? new ReplyKeyboardMarkup(new[]
                {
                    new[] { new KeyboardButton("/showalltasks"), new KeyboardButton("/showtasks") },
                    new[] { new KeyboardButton("/report") }
                })
                { ResizeKeyboard = true }
                : new ReplyKeyboardMarkup(new[]
                {
                    new[] { new KeyboardButton("/start") }
                })
                { ResizeKeyboard = true };
        }
    }
}
