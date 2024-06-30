using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.blueprint.runtime;
using MongoDB.Driver.GeoJsonObjectModel;

namespace blueprint.modules.blueprint.core
{
    public class Blueprint
    {
        public string id { get; set; }
        public Process _process { get; set; }
        public List<Field> fields { get; private set; }
        public List<Block> blocks { get; private set; }
        public IEnumerable<blocks.Node> nodes
        {
            get
            {
                return blocks.Where(i => (i is blocks.Node)).Select(i => (blocks.Node)i);
            }
        }
        public Blueprint()
        {
            fields = new List<Field>();
            blocks = new List<Block>();
        }
        public blocks.Node FindNodeWithName(string name)
        {
            return nodes.FirstOrDefault(i => i.name == name);
        }
        public blocks.Node FindNodeWithId(string id)
        {
            return nodes.FirstOrDefault(i => i.id == id);
        }

        public void AddBlock(Block block)
        {
            block.parent = this;
            blocks.Add(block);
        }
        public void RemoveBlock(string id)
        {
            var block = blocks.FirstOrDefault(i => i.id == id);
            if (block != null)
                blocks.Remove(block);
        }
        public void AddEnvField(Field field)
        {
            field.parent = this;
            fields.Add(field);
        }

        public T FindComponent<T>() where T : ComponentBase
        {
            foreach (var n in nodes)
            {
                var c = n.GetComponent<T>();
                if (c != null)
                    return c;
            }
            return default;
        }
        public List<T> FindComponents<T>() where T : ComponentBase
        {
            var list = new List<T>();
            foreach (var n in nodes)
            {
                var c = n.GetComponent<T>();
                if (c != null)
                    list.Add(c);
            }
            return list;
        }
    }
}
