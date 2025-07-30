using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Core.Services;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using ToDoListConsoleBot.Scenarios;

namespace ToDoListConsoleBot.Bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _reportService;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IEnumerable<IScenario> _scenarios;

        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService toDoService,
            IToDoReportService reportService,
            IScenarioContextRepository contextRepository,
            IEnumerable<IScenario> scenarios)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _reportService = reportService;
            _contextRepository = contextRepository;
            _scenarios = scenarios;
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

                // Обработка команды /cancel до любых сценариев
                if (text == "/cancel")
                {
                    await _contextRepository.ResetContext(from.Id, cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId, "Сценарий отменён.", replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                    return;
                }

                // Обработка активного сценария
                var context = await _contextRepository.GetContext(from.Id, cancellationToken);
                if (context is not null)
                {
                    await ProcessScenario(context, update, cancellationToken);
                    return;
                }

                // Обработка команд
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
                    var scenarioContext = new ScenarioContext(from.Id, ScenarioType.AddTask);
                    await _contextRepository.SetContext(from.Id, scenarioContext, cancellationToken);
                    await ProcessScenario(scenarioContext, update, cancellationToken);
                    return;
                }

                if (text.StartsWith("/removetask"))
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
                        "/addtask – добавить задачу (сценарий)",
                        "/removetask [ID] – удалить задачу",
                        "/completetask [ID] – завершить задачу",
                        "/showtasks – активные задачи",
                        "/showalltasks – все задачи",
                        "/report – статистика",
                        "/find [префикс] – поиск задач",
                        "/cancel – отменить текущий сценарий",
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
                await _botClient.SendTextMessageAsync(update.Message!.Chat.Id, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
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
                    new[] { new KeyboardButton("/addtask") },
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

        private IScenario GetScenario(ScenarioType scenario)
        {
            return _scenarios.FirstOrDefault(s => s.CanHandle(scenario))
                   ?? throw new InvalidOperationException($"Сценарий {scenario} не поддерживается.");
        }

        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(context.UserId, ct);
                var keyboard = BuildKeyboard(true);
                await _botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Сценарий завершён.", replyMarkup: keyboard, cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(context.UserId, context, ct);
                // Клавиатура /cancel остаётся
            }
        }
    }
}
