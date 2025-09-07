using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;

using ToDoListConsoleBot.Scenarios;

namespace ToDoListConsoleBot.BackgroundTasks
{
    public class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot
        ) : base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var contexts = await _scenarioRepository.GetContexts(ct);

            foreach (var context in contexts)
            {
                if (DateTime.UtcNow - context.CreatedAt > _resetScenarioTimeout)
                {
                    await _scenarioRepository.ResetContext(context.UserId, ct);

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "/addtask", "/show", "/report" }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await _bot.SendTextMessageAsync(
                        chatId: context.UserId,
                        text: $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                        replyMarkup: keyboard,
                        cancellationToken: ct
                    );
                }
            }
        }
    }
}
