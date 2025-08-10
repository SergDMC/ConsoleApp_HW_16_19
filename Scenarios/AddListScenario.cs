using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using ToDoListConsoleBot.Core.Services;

using ToDoListConsoleBot.Models;

namespace ToDoListConsoleBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;

        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

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
                        await botClient.SendTextMessageAsync(chatId, "Введите название списка:", cancellationToken: ct);
                        context.CurrentStep = "Name";
                        return ScenarioResult.Transition;
                    }

                case "Name":
                    {
                        var user = (ToDoUser)context.Data!;
                        var name = update.Message?.Text;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            await botClient.SendTextMessageAsync(chatId, "Название списка не может быть пустым. Попробуйте ещё раз:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        await _toDoListService.Add(user, name!, ct);
                        await botClient.SendTextMessageAsync(chatId, $"Список '{name}' успешно добавлен.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}
