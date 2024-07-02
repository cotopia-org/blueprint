using blueprint.core;
using blueprint.modules.database.logic;
using blueprint.modules.scheduler.database;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;

namespace blueprint.modules.scheduler.logic
{
    public class SchedulerModule : Module<SchedulerModule>
    {

        public IMongoCollection<schedule> dbContext { get; private set; }
        public event Action<SchedulerResponse> OnAction;
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<schedule>("schedule");
            LoopProcess();
            Indexing();
        }
        private async void Indexing()
        {
            try
            {
                var builder = Builders<schedule>.IndexKeys.Ascending(i => i.invokeTime);
                await dbContext.Indexes.CreateOneAsync(new CreateIndexModel<schedule>(builder, new CreateIndexOptions() { Background = true }));
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        private async void LoopProcess()
        {
            while (true)
            {
                int wait = 100;
                List<database.schedule> schedules;
                try
                {
                    schedules = await dbContext
                         .AsQueryable()
                         .Where(i => i.invokeTime < DateTime.UtcNow)
                         .Take(50)
                         .ToListAsync();

                    if (schedules.Count > 0)
                    {
                        wait = 1;
                        await dbContext
                            .DeleteManyAsync(Builders<schedule>.Filter
                            .In(i => i._id, schedules.Select(i => i._id).ToList()) &
                            Builders<schedule>.Filter
                            .Eq(i => i.repeat, false));

                        var now = DateTime.UtcNow;

                        foreach (var schedule in schedules)
                        {
                            if (schedule.repeat)
                            {
                                var delay = schedule.invokeTime - schedule.createDateTime;
                                var createTime = schedule.invokeTime;
                                var invokeTime = createTime + delay;

                                if( invokeTime < DateTime.UtcNow)
                                {
                                    createTime = DateTime.UtcNow;
                                    invokeTime = createTime + delay;
                                }

                                await dbContext.UpdateOneAsync(
                                 Builders<schedule>.Filter.Eq(i => i.key, schedule.key),
                                   Builders<schedule>.Update
                                   .Set(i => i.invokeTime, invokeTime)
                                   .Set(i => i.createDateTime, createTime)
                                 );
                            }

                            try
                            {
                                ExucuteSchedule(schedule);
                            }
                            catch (Exception e)
                            {
                                Debug.Error(e);
                            }
                        }
                    }
                    else
                    {
                        wait = 1000;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.StartsWith("A timeout occured after"))
                        Debug.Warning("[DB] Connection problem.");
                    else
                        Debug.Error(e);
                }

                await Task.Delay(wait);
            }
        }
        private void ExucuteSchedule(database.schedule schedule)
        {
            var s = new SchedulerResponse();
            s.id = schedule._id.ToString();
            s.key = schedule.key;
            s.payload = schedule.payload;
            s.category = schedule.category;
            s.repeat = schedule.repeat;
            s.createDateTime = schedule.createDateTime;
            s.invokeTime = schedule.invokeTime;

            OnAction?.Invoke(s);
        }
        public async void Upsert(string key, TimeSpan delayTime, string payload, string category, bool repeat = false)
        {
            try
            {
                var now = DateTime.UtcNow;

                await dbContext.UpdateOneAsync(
                Builders<schedule>.Filter.Eq(i => i.key, key),
                  Builders<schedule>.Update
                  .Set(i => i.key, key)
                  .Set(i => i.category, category)
                  .Set(i => i.repeat, repeat)
                  .Set(i => i.payload, payload)
                  .Set(i => i.invokeTime, now + delayTime)
                  .Set(i => i.createDateTime, now)
                  , new UpdateOptions() { IsUpsert = true }
                );
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async void Remove(string key)
        {
            try
            {
                await dbContext.DeleteOneAsync(i => i.key == key);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
    }

}
