using Core.DataAccess;
using Core.Services;

public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _repository;

    public ToDoReportService(IToDoRepository repository)
    {
        _repository = repository;
    }

    public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        var items = _repository.GetAllByUserId(userId);
        int total = items.Count;
        int active = items.Count(x => x.IsActive);
        int completed = total - active;
        return (total, completed, active, DateTime.Now);
    }
}
