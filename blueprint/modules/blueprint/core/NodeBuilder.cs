using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.node.database;
using blueprint.modules.node.types;
using srtool;

namespace blueprint.modules.blueprint.core
{
    public class NodeBuilder
    {
        public string id => ObjectIdExtension.GenerateBySeed($"nodeBuilder:{GetType().Name}").ToString();
        public string name { get; set; }
        public string title { get; set; }
        public string script { get; set; }
        public List<NodeField> fields { get; set; }
        public  List<Component> components { get; set; }
        public virtual void Build()
        {
            components = new List<Component>();
            fields = new List<NodeField>();
        }
        public void AddComponent(Component component)
        {
            components.Add(component);
        }
        public void AddField(NodeField field)
        {
            fields.Add(field);
        }
    }
}
