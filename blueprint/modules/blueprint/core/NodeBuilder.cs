using blueprint.modules.blueprint.core.blocks;
using srtool;

namespace blueprint.modules.blueprint.core
{
    public class NodeBuilder
    {
        public virtual string id => ObjectIdExtension.GenerateBySeed($"nodeBuilder:{GetType().Name}").ToString();
        public virtual string name { get; }
        public virtual string title { get; }
        public virtual string script { get; }
        public virtual Node Node()
        {
            var node = new Node();
            node.id = id;
            node.name = name;
            node.script = new Script(script);

            return node;
        }
    }
}
