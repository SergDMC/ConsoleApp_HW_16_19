using Core.Services;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _repository;

    public ToDoReportService(IToDoRepository repository)
    {
        _repository = repository;
    }

    public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllByUserIdAsync(userId, cancellationToken);
        int total = items.Count;
        int active = items.Count(x => x.IsActive);
        int completed = total - active;
        return (total, completed, active, DateTime.UtcNow);
    }
}
