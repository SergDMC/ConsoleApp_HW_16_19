using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using ToDoListConsoleBot.Core.Entities;
using ToDoListConsoleBot.Core.Services;
using ToDoListConsoleBot.Models;
using ToDoListConsoleBot.TelegramBot.Dto;

namespace ToDoListConsoleBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;

        public AddTaskScenario(
            IUserService userService,
            IToDoListService toDoListService,
            IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
            => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                // Первый шаг — выбор списка
                case null:
                    {
                        var message = update.Message;
                        if (message == null)
                            return ScenarioResult.Transition;

                        var user = await _userService.GetOrCreateUserAsync(message.From!.Id, ct);
                        context.Data["user"] = user;

                        var lists = await _toDoListService.GetByUserId(user.UserId, ct);

                        var buttons = lists
                            .Select(l => InlineKeyboardButton.WithCallbackData(
                                l.Name,
                                new ToDoListCallbackDto
                                {
                                    Action = "addtask",
                                    ToDoListId = l.Id
                                }.ToString()))
                            .ToList();

                        // Кнопка "Без списка"
                        buttons.Insert(0, InlineKeyboardButton.WithCallbackData(
                            "📌Без списка",
                            new ToDoListCallbackDto
                            {
                                Action = "addtask",
                                ToDoListId = null
                            }.ToString()));

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Выберите список для задачи:",
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: ct);

                        context.CurrentStep = "List";
                        return ScenarioResult.Transition;
                    }

                // Обработка выбора списка через CallbackQuery
                case "List":
                    {
                        if (update.CallbackQuery == null)
                            return ScenarioResult.Continue;

                        var dto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data!);
                        context.Data["toDoListId"] = dto.ToDoListId;

                        await bot.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message!.Chat.Id,
                            text: "Введите название задачи:",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                            new[] { new KeyboardButton("/cancel") }
                            })
                            { ResizeKeyboard = true },
                            cancellationToken: ct);

                        context.CurrentStep = "Name";
                        return ScenarioResult.Transition;
                    }

                // Получение названия задачи
                case "Name":
                    {
                        var message = update.Message;
                        if (message == null)
                            return ScenarioResult.Transition;

                        context.Data["taskName"] = message.Text ?? "";

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Введите дедлайн задачи (формат: dd.MM.yyyy):",
                            cancellationToken: ct);

                        context.CurrentStep = "Deadline";
                        return ScenarioResult.Transition;
                    }

                // Получение дедлайна и сохранение задачи
                case "Deadline":
                    {
                        var message = update.Message;
                        if (message == null)
                            return ScenarioResult.Transition;

                        if (!DateTime.TryParseExact(message.Text, "dd.MM.yyyy", null,
                            System.Globalization.DateTimeStyles.None, out var deadline))
                        {
                            await bot.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:",
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        var user = (ToDoUser)context.Data["user"];
                        var taskName = (string)context.Data["taskName"];
                        var toDoListId = (Guid?)context.Data["toDoListId"];

                        await _toDoService.AddAsync(user, taskName, deadline, toDoListId, ct);

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Задача успешно добавлена.",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                            new[] { new KeyboardButton("/addtask"), new KeyboardButton("/show") }
                            })
                            { ResizeKeyboard = true },
                            cancellationToken: ct);

                        return ScenarioResult.Completed;
                    }

                default:
                    if (update.Message != null)
                    {
                        await bot.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Произошла ошибка. Сценарий сброшен.",
                            cancellationToken: ct);
                    }
                    return ScenarioResult.Completed;
            }
        }
    }
}
