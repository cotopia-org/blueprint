
namespace blueprint.modules.blueprint.core.component
{
    public class Pulse : ComponentBase
    {
        public origin origin { get; set; }
        public string delayParam { get; set; }
        public string callback { get; set; }
    }
    public enum origin
    {
        none,
        min,
        hour,
        day,
        month
    }
}
