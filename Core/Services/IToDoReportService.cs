using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
