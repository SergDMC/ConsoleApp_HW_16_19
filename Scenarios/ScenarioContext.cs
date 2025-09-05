using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Scenarios
{
    public class ScenarioContext
    {
        public long UserId { get; }
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
            Data = new Dictionary<string, object>();
        }

        public ScenarioContext(long userId, ScenarioType scenario) : this(scenario)
        {
            UserId = userId;
        }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}
