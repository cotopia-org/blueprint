using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.database;
using MongoDB.Driver;
using srtool;

namespace blueprint.modules.blueprint
{
    public partial class BlueprintModule
    {
        public class BlueprintSaveHandler
        {
            public Blueprint blueprint;
            bool saving = false;
            public BlueprintSaveHandler(Blueprint blueprint)
            {
                this.blueprint = blueprint;
            }
            public async void doSave(Blueprint blueprint)
            {
                try
                {
                    if (!saving)
                    {
                        saving = true;
                        await Task.Delay(10000);

                        await BlueprintModule.Instance.dbContext.UpdateOneAsync(
                            Builders<blueprint_model>.Filter.Eq(i => i._id, blueprint.id),
                            Builders<blueprint_model>.Update.Set(i => i.data_snapshot, blueprint.Snapshot()));

                        saving = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.Error(e);
                    saving = false;
                }

            }
        }
    }
}
