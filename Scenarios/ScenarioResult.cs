using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListConsoleBot.Scenarios
{
    public enum ScenarioResult
    {
        Transition, // Пер. к следующему шагу
        Completed   // Завершение сценария
    }
}
