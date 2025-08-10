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
using ToDoListConsoleBot.TelegramBot.Dto;
using ToDoListConsoleBot.Core.Services;

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
        private readonly IToDoListService _toDoListService;

        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService toDoService,
            IToDoReportService reportService,
            IScenarioContextRepository contextRepository,
            IEnumerable<IScenario> scenarios,
            IToDoListService toDoListService)

        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _reportService = reportService;
            _contextRepository = contextRepository;
            _scenarios = scenarios;
            _toDoListService = toDoListService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
                return;
            }

            if (update.Message?.Text is { } text && update.Message.From != null)
            {
                await HandleMessageAsync(update.Message, cancellationToken);
            }
        }

        private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var text = message.Text!;
            var from = message.From!;
            var chatId = message.Chat.Id;

            try
            {
                var currentUser = await _userService.GetUserAsync(from.Id, cancellationToken);
                var keyboardMarkup = BuildKeyboard(currentUser != null);

                if (text == "/cancel")
                {
                    await _contextRepository.ResetContext(from.Id, cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId, "Сценарий отменён.", replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                    return;
                }

                var context = await _contextRepository.GetContext(from.Id, cancellationToken);
                if (context != null)
                {
                    await ProcessScenario(context, new Update { Message = message }, cancellationToken);
                    return;
                }

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

                if (text == "/show")
                {
                    var lists = await _toDoListService.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);

                    var buttons = new List<List<InlineKeyboardButton>>();

                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData(
                            "📌Без списка",
                            new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString())
                    });

                    foreach (var list in lists)
                    {
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData(
                                list.Name,
                                new ToDoListCallbackDto { Action = "show", ToDoListId = list.Id }.ToString())
                        });
                    }

                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist"),
                        InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist")
                    });

                    await _botClient.SendTextMessageAsync(
                        chatId,
                        "Выберите список",
                        replyMarkup: new InlineKeyboardMarkup(buttons),
                        cancellationToken: cancellationToken);

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
                        "/show - выбрать список",
                        "/addtask – добавить задачу (сценарий)",
                        "/removetask [ID] – удалить задачу",
                        "/completetask [ID] – завершить задачу",
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
                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
            }

            private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
            {
                var from = callbackQuery.From;
                if (from == null)
                    return;

                var currentUser = await _userService.GetUserAsync(from.Id, cancellationToken);
                if (currentUser == null)
                {
                    
                    await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                        "Пожалуйста, зарегистрируйтесь через команду /start.",
                        cancellationToken: cancellationToken);
                    return;
                }

                if (string.IsNullOrEmpty(callbackQuery.Data))
                    return;

                try
                {
                    var dto = CallbackDto.FromString(callbackQuery.Data);

                    if (dto.Action == "show")
                    {
                        var toDoListDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
                        IReadOnlyList<ToDoListConsoleBot.Core.Entities.ToDoItem> tasks;

                        if (toDoListDto.ToDoListId == null)
                        {
                            // Получаем задачи без списка
                            tasks = await _toDoService.GetActiveByUserIdWithoutListAsync(currentUser.UserId, cancellationToken);
                        }
                        else
                        {
                            // Получаем задачи конкретного списка
                            tasks = await _toDoService.GetActiveByListIdAsync(toDoListDto.ToDoListId.Value, cancellationToken);
                        }

                        if (tasks.Count == 0)
                        {
                            await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id,
                                "Задачи не найдены.", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            foreach (var task in tasks)
                            {
                                var status = task.IsActive ? "Активна" : "Завершена";
                                var text = $"[{task.Id}] {task.Name} - {status}";
                                await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, text, cancellationToken: cancellationToken);
                            }
                        }
                    }
                    else if (dto.Action == "addlist")
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id,
                            "Добавить новый список", cancellationToken: cancellationToken);
                    }
                    else if (dto.Action == "deletelist")
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id,
                            "Удалить список", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id,
                            $"Неизвестное действие: {dto.Action}", cancellationToken: cancellationToken);
                    }

                    await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                }
                catch
                {
                    await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка обработки кнопки", cancellationToken: cancellationToken);
                }
            }


            private static IReplyMarkup BuildKeyboard(bool isRegistered)
            {
                return isRegistered
                    ? new ReplyKeyboardMarkup(new[]
                    {
                    new[] { new KeyboardButton("/addtask") },
                    new[] { new KeyboardButton("/show") },
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

                }
            }
        }
    }
}
