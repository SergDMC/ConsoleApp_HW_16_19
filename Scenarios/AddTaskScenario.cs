using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
            => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var message = update.Message;
            if (message == null)
                return ScenarioResult.Transition;

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetOrCreateUserAsync(message.From!.Id, ct);
                        context.Data["user"] = user;

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
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

                case "Name":
                    {
                        context.Data["taskName"] = message.Text ?? "";

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Введите дедлайн задачи (формат: dd.MM.yyyy):",
                            cancellationToken: ct);

                        context.CurrentStep = "Deadline";
                        return ScenarioResult.Transition;
                    }

                case "Deadline":
                    {
                        if (!DateTime.TryParseExact(message.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var deadline))
                        {
                            await bot.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:",
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        var user = (ToDoUser)context.Data["user"];
                        var taskName = (string)context.Data["taskName"];

                        await _toDoService.AddAsync(user, taskName, deadline, ct);

                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Задача успешно добавлена.",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                        new[] { new KeyboardButton("/addtask"), new KeyboardButton("/showtask") }
                            })
                            { ResizeKeyboard = true },
                            cancellationToken: ct);

                        return ScenarioResult.Completed;
                    }

                default:
                    await bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Произошла ошибка. Сценарий сброшен.",
                        cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}
