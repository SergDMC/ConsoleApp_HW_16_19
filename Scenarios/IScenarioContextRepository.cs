using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Scenarios
{
    public interface IScenarioContextRepository
    {
        Task<ScenarioContext?> GetContext(long userId, CancellationToken ct);
        Task SetContext(long userId, ScenarioContext context, CancellationToken ct);
        Task ResetContext(long userId, CancellationToken ct);
    }
}
