using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using Microsoft.ClearScript.V8;

namespace blueprint.modules.blueprint.core
{
    public class Expression
    {
        private Script script { get; set; }
        public string expression { get; private set; }
        public Expression()
        {
        } 
        public Expression(string expression)
        {
            this.expression = expression;
            script = new Script(this.expression);
        }
        public object Value(ScriptInput input)
        {
            return script.AsExpression(input);
        }
    }
}
