using System;
using System.Threading.Tasks;

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

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            try
            {
                var message = update.Message;
                var user = message.From;
                var chatId = message.Chat.Id;
                var text = message.Text?.Trim();

                if (string.IsNullOrEmpty(text))
                    return;

                var isRegistered = _userService.GetUser(user.Id) != null;

                if (text.StartsWith("/start"))
                {
                    if (!isRegistered)
                    {
                        var newUser = _userService.RegisterUser(user.Id, user.Username);
                        await _botClient.SendMessage(chatId, $"Привет, {newUser.TelegramUserName}! Вы успешно зарегистрированы.");
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Вы уже зарегистрированы.");
                    }
                    return;
                }

                if (!isRegistered && text != "/help" && text != "/info")
                {
                    await _botClient.SendMessage(chatId, "Пожалуйста, зарегистрируйтесь с помощью команды /start.");
                    return;
                }

                var currentUser = _userService.GetUser(user.Id)!;

                if (text.StartsWith("/addtask"))
                {
                    var taskName = text.Substring("/addtask".Length).Trim();
                    if (string.IsNullOrEmpty(taskName))
                    {
                        await _botClient.SendMessage(chatId, "Пожалуйста, укажите название задачи.");
                        return;
                    }

                    var item = _toDoService.Add(currentUser, taskName);
                    await _botClient.SendMessage(chatId, $"Задача добавлена: {item.Name}");
                }
                else if (text.StartsWith("/removetask"))
                {
                    var idStr = text.Substring("/removetask".Length).Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        _toDoService.Delete(id);
                        await _botClient.SendMessage(chatId, "Задача удалена.");
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Неверный формат ID задачи.");
                    }
                }
                else if (text.StartsWith("/completetask"))
                {
                    var idStr = text.Substring("/completetask".Length).Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        _toDoService.MarkCompleted(id);
                        await _botClient.SendMessage(chatId, "Задача отмечена как выполненная.");
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Неверный формат ID задачи.");
                    }
                }
                else if (text.StartsWith("/showtasks"))
                {
                    var tasks = _toDoService.GetActiveByUserId(currentUser.UserId);
                    if (tasks.Count == 0)
                    {
                        await _botClient.SendMessage(chatId, "У вас нет активных задач.");
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            await _botClient.SendMessage(chatId, $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}");
                        }
                    }
                }
                else if (text.StartsWith("/showalltasks"))
                {
                    var tasks = _toDoService.GetAllByUserId(currentUser.UserId);
                    if (tasks.Count == 0)
                    {
                        await _botClient.SendMessage(chatId, "У вас нет задач.");
                    }
                    else
                    {
                        foreach (var task in tasks)
                        {
                            await _botClient.SendMessage(chatId, $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}");
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
                                   "/help - показать это сообщение";
                    await _botClient.SendMessage(chatId, helpText);
                }
                else if (text.StartsWith("/info"))
                {
                    await _botClient.SendMessage(chatId, "Это консольный ToDo бот. Используйте команды для управления задачами.");
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Неизвестная команда. Используйте /help для списка доступных команд.");
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(update.Message.Chat.Id, $"Произошла ошибка: {ex.Message}");
            }
        }

        void IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            throw new NotImplementedException();
        }
    }
}
