using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace blueprint.modules.blueprint.core.blocks
{
    public class Block
    {
        public string reference_id { get; set; }
        public object parent { get; set; }
        public Blueprint bind_blueprint
        {
            get
            {
                if (parent == null)
                    return null;
                else
                    return parent as Blueprint;
            }
        }
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public Coordinate coordinate { get; set; }


        public Block()
        {
        }


    }
}
