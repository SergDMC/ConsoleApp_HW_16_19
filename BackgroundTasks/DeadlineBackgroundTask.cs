using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.Services;

namespace ToDoListConsoleBot.BackgroundTasks
{
    public class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public DeadlineBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromHours(1), nameof(DeadlineBackgroundTask))
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
                var tasks = await _toDoRepository.GetActiveWithDeadline(
                    user.UserId,
                    DateTime.UtcNow.AddDays(-1).Date,
                    DateTime.UtcNow.Date,
                    ct);

                foreach (var task in tasks)
                {
                    var type = $"Deadline_{task.Id}";
                    var text = $"Вы пропустили дедлайн по задаче {task.Name}!";
                    await _notificationService.ScheduleNotification(user.UserId, type, text, DateTime.UtcNow, ct);
                }
            }
        }
    }
}
