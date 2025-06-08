using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Core.Services;

using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

using ToDoListConsoleBot.Models;
using ToDoListConsoleBot.Services;

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

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = update.Message;
                var user = message.From;
                var chatId = message.Chat.Id;
                var text = message.Text?.Trim();

                if (string.IsNullOrEmpty(text))
                    return;

                var currentUser = await _userService.GetUserAsync(user.Id, cancellationToken);

                if (text.StartsWith("/start"))
                {
                    if (currentUser == null)
                    {
                        var newUser = await _userService.RegisterUserAsync(user.Id, user.Username, cancellationToken);
                        await _botClient.SendMessage(chatId, $"Привет, {newUser.TelegramUserName}! Вы успешно зарегистрированы.", cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Вы уже зарегистрированы.", cancellationToken);
                    }
                    return;
                }

                if (currentUser == null && text != "/help" && text != "/info")
                {
                    await _botClient.SendMessage(chatId, "Пожалуйста, зарегистрируйтесь с помощью команды /start.", cancellationToken);
                    return;
                }

                if (text.StartsWith("/addtask"))
                {
                    var taskName = text[8..].Trim();
                    if (string.IsNullOrEmpty(taskName))
                    {
                        await _botClient.SendMessage(chatId, "Пожалуйста, укажите название задачи.", cancellationToken);
                        return;
                    }

                    var item = await _toDoService.AddAsync(currentUser, taskName, cancellationToken);
                    await _botClient.SendMessage(chatId, $"Задача добавлена: {item.Name}", cancellationToken);
                }
                else if (text.StartsWith("/removetask"))
                {
                    var idStr = text[11..].Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        await _toDoService.DeleteAsync(id, cancellationToken);
                        await _botClient.SendMessage(chatId, "Задача удалена.", cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Неверный формат ID задачи.", cancellationToken);
                    }
                }
                else if (text.StartsWith("/completetask"))
                {
                    var idStr = text[14..].Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        await _toDoService.MarkCompletedAsync(id, cancellationToken);
                        await _botClient.SendMessage(chatId, "Задача отмечена как выполненная.", cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Неверный формат ID задачи.", cancellationToken);
                    }
                }
                else if (text.StartsWith("/showtasks"))
                {
                    var tasks = await _toDoService.GetActiveByUserIdAsync(currentUser.UserId, cancellationToken);
                    if (tasks.Count == 0)
                    {
                        await _botClient.SendMessage(chatId, "У вас нет активных задач.", cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            await _botClient.SendMessage(chatId, $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}", cancellationToken);
                        }
                    }
                }
                else if (text.StartsWith("/showalltasks"))
                {
                    var tasks = await _toDoService.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
                    if (tasks.Count == 0)
                    {
                        await _botClient.SendMessage(chatId, "У вас нет задач.", cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            await _botClient.SendMessage(chatId, $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}", cancellationToken);
                        }
                    }
                }
                else if (text.StartsWith("/report"))
                {
                    var stats = await _reportService.GetUserStatsAsync(currentUser.UserId, cancellationToken);
                    await _botClient.SendMessage(chatId, $"Статистика по задачам на {stats.generatedAt:G}. Всего: {stats.total}; Завершённых: {stats.completed}; Активных: {stats.active}.", cancellationToken);
                }
                else if (text.StartsWith("/find"))
                {
                    var prefix = text[5..].Trim();
                    if (string.IsNullOrEmpty(prefix))
                    {
                        await _botClient.SendMessage(chatId, "Укажите префикс имени задачи: /find <prefix>", cancellationToken);
                        return;
                    }

                    var tasks = await _toDoService.FindAsync(currentUser, prefix, cancellationToken);
                    if (!tasks.Any())
                    {
                        await _botClient.SendMessage(chatId, "Задачи не найдены.", cancellationToken);
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            await _botClient.SendMessage(chatId, $"[{task.Id}] {task.Name} - {(task.IsActive ? "Активна" : "Завершена")}", cancellationToken);
                        }
                    }
                }
                else if (text.StartsWith("/help"))
                {
                    var helpText = "/addtask [название задачи] - добавить новую задачу\n" +
                                   "/removetask [ID задачи] - удалить задачу\n" +
                                   "/completetask [ID задачи] - отметить задачу как выполненную\n" +
                                   "/showtasks - показать активные задачи\n" +
                                   "/showalltasks - показать все задачи\n" +
                                   "/report - статистика задач\n" +
                                   "/find [префикс] - найти задачи по имени\n" +
                                   "/help - показать это сообщение";
                    await _botClient.SendMessage(chatId, helpText, cancellationToken);
                }
                else if (text.StartsWith("/info"))
                {
                    await _botClient.SendMessage(chatId, "Это консольный ToDo бот. Используйте команды для управления задачами.", cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Неизвестная команда. Используйте /help для списка доступных команд.", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(update.Message.Chat.Id, $"Произошла ошибка: {ex.Message}", cancellationToken);
            }
        }
    }
}
