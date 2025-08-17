using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using ToDoListConsoleBot.Core.Services;
using ToDoListConsoleBot.Models;
using ToDoListConsoleBot.TelegramBot.Dto;

namespace ToDoListConsoleBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUserAsync(context.UserId, ct);
                        if (user == null)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Пользователь не найден. Пожалуйста, зарегистрируйтесь.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        context.Data = user;

                        var lists = await _toDoListService.GetAllByUserIdAsync(user.UserId, ct);
                        if (lists.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(chatId, "У вас нет списков для удаления.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var buttons = lists
                            .Select(l => new List<InlineKeyboardButton>
                            {
                            InlineKeyboardButton.WithCallbackData(
                                l.Name,
                                new ToDoListCallbackDto { Action = "deletelist", ToDoListId = l.Id }.ToString())
                            }).ToList();

                        await botClient.SendTextMessageAsync(chatId,
                            "Выберите список для удаления:",
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: ct);

                        context.CurrentStep = "Approve";
                        return ScenarioResult.Transition;
                    }

                case "Approve":
                    {
                        if (update.CallbackQuery == null || string.IsNullOrEmpty(update.CallbackQuery.Data))
                        {
                            await botClient.SendTextMessageAsync(chatId, "Ошибка: не получены данные для удаления.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var dto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);

                        if (dto.Action != "deletelist" || dto.ToDoListId == null)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Некорректные данные.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var list = await _toDoListService.GetByIdAsync(dto.ToDoListId.Value, ct);
                        if (list == null)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Список не найден.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        context.Data = list;

                        var confirmButtons = new InlineKeyboardMarkup(new[]
                        {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                        }
                    });

                        await botClient.SendTextMessageAsync(chatId,
                            $"Подтверждаете удаление списка '{list.Name}' и всех его задач?",
                            replyMarkup: confirmButtons,
                            cancellationToken: ct);

                        context.CurrentStep = "Delete";

                        // Обязательно ответить на CallbackQuery, чтобы не висел часик в Telegram
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, cancellationToken: ct);

                        return ScenarioResult.Transition;
                    }

                case "Delete":
                    {
                        if (update.CallbackQuery == null || string.IsNullOrEmpty(update.CallbackQuery.Data))
                        {
                            await botClient.SendTextMessageAsync(chatId, "Ошибка: не получены данные для удаления.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var data = update.CallbackQuery.Data;

                        var list = context.Data as ToDoListConsoleBot.Core.Entities.ToDoList;
                        if (list == null)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Ошибка: данные списка не найдены.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        if (data == "yes")
                        {
                            var user = (ToDoUser)context.Data!;
                            user = await _userService.GetUserAsync(context.UserId, ct) ?? user;

                            // Удаляем задачи в списке
                            await _toDoService.DeleteAllByUserAndListAsync(context.UserId, list.Id, ct);

                            // Удаляем список
                            await _toDoListService.DeleteAsync(list.Id, ct);

                            await botClient.SendTextMessageAsync(chatId, $"Список '{list.Name}' и все его задачи удалены.", cancellationToken: ct);
                        }
                        else if (data == "no")
                        {
                            await botClient.SendTextMessageAsync(chatId, "Удаление отменено.", cancellationToken: ct);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "Неверный ответ.", cancellationToken: ct);
                        }

                        return ScenarioResult.Completed;
                    }

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}
