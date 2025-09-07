using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Core.Services
{
    public interface INotificationService
    {
        Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct);
        Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);
        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}
