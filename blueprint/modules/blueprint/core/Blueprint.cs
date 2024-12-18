﻿using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.blueprint.runtime;
using blueprint.modules.node.types;
using Microsoft.AspNetCore.JsonPatch.Internal;
using MongoDB.Driver.GeoJsonObjectModel;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace blueprint.modules.blueprint.core
{
    public class Blueprint
    {
        public event Action<Blueprint> onChangeStaticData;
        public Process _process { get; set; }

        public string id { get; set; }
        public Dictionary<string, Field> fields { get; private set; }
        public List<Block> blocks { get; private set; }
        public Blueprint source { get; set; }
        public IEnumerable<blocks.Node> nodes
        {
            get
            {
                return blocks.Where(i => (i is blocks.Node)).Select(i => (blocks.Node)i);
            }
        }
        public Blueprint()
        {
            fields = new Dictionary<string, Field>();
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
        public void AddEnvField(string name, Field field)
        {
            fields.Add(name, field);
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


        public void InvokeOnChangeStaticData()
        {
            onChangeStaticData?.Invoke(this);
        }

        public void SetWebResponse(WebResponse webResponse)
        {
            this.webResponse = webResponse;

            if (_WaitForWebResponseToken != null)
                _WaitForWebResponseToken.Cancel();
        }
        private WebResponse webResponse;
        private Semaphore semaphore= new Semaphore(1,1);
        private CancellationTokenSource _WaitForWebResponseToken;
        public async Task<WebResponse> WaitForWebResponse(TimeSpan timeout)
        {
            if (this.webResponse != null)
            {
                return this.webResponse;
            }
            else
            {
              //  semaphore.
                _WaitForWebResponseToken = new CancellationTokenSource();
                try
                {
                    await Task.Delay(timeout, _WaitForWebResponseToken.Token);

                    return null;
                }
                catch (TaskCanceledException)
                {
                    return this.webResponse;
                }
            }

        }
    }
    public class WebResponse
    {
        public int statusCode { get; set; }
        public string Content { get; set; }
    }

}
