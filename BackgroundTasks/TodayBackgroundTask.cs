using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.Services;

namespace ToDoListConsoleBot.BackgroundTasks
{
    public class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public TodayBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);

            foreach (var user in users)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var tasks = await _toDoRepository.GetActiveWithDeadline(
                    user.UserId,
                    today.ToDateTime(TimeOnly.MinValue),
                    today.ToDateTime(TimeOnly.MaxValue),
                    ct);

                if (tasks.Any())
                {
                    var taskList = string.Join("\n", tasks.Select(t => $"- {t.Name}"));
                    var type = $"Today_{today}";
                    var text = $"Ваши задачи на сегодня:\n{taskList}";

                    await _notificationService.ScheduleNotification(user.UserId, type, text, DateTime.UtcNow, ct);
                }
            }
        }
    }
}
