using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

using Telegram.Bot;

namespace ToDoListConsoleBot.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _toDoService;

        public DeleteTaskScenario(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (!context.Data.ContainsKey("TaskId"))
                return ScenarioResult.Completed;

            var taskId = Guid.Parse(context.Data["TaskId"].ToString()!);

            if (update.CallbackQuery == null)
            {
                var buttons = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Да", $"confirmdelete|{taskId}"),
                        InlineKeyboardButton.WithCallbackData("Нет", "canceldelete")
                    }
                });

                await bot.SendTextMessageAsync(update.CallbackQuery!.Message!.Chat.Id,
                    "Вы уверены, что хотите удалить задачу?",
                    replyMarkup: buttons,
                    cancellationToken: ct);

                return ScenarioResult.InProgress;
            }

            if (update.CallbackQuery.Data!.StartsWith("confirmdelete"))
            {
                await _toDoService.DeleteAsync(taskId, ct);
                await bot.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Задача удалена ❌",
                    cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            if (update.CallbackQuery.Data == "canceldelete")
            {
                await bot.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Удаление отменено",
                    cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            return ScenarioResult.InProgress;
        }
    }
}
