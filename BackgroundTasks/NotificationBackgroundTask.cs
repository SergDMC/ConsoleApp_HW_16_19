using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

using ToDoListConsoleBot.Core.Services;

namespace ToDoListConsoleBot.BackgroundTasks
{
    public class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _bot;

        public NotificationBackgroundTask(INotificationService notificationService, ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);

            foreach (var n in notifications)
            {
                await _bot.SendTextMessageAsync(
                    chatId: n.User.TelegramUserId,
                    text: n.Text,
                    cancellationToken: ct);

                await _notificationService.MarkNotified(n.Id, ct);
            }
        }
    }
}
