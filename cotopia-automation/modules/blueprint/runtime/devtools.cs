
using Microsoft.ClearScript;

namespace blueprint.modules.blueprint.runtime
{
    public static class devtools
    {
        public static async Task delay(double sec, ScriptObject action)
        {
            await Task.Delay(TimeSpan.FromSeconds(sec));
            action.InvokeAsFunction(new object[] { });
        }
    }
}
