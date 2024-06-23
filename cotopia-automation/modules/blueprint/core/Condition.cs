namespace blueprint.modules.blueprint.core
{
    public class Condition
    {
        public ConditionType type { get; set; }
        public List<ConditionExpression> expressions { get; set; }
    }
}
