using blueprint.core;
using blueprint.modules.database.logic;
using blueprint.modules.schedule.database;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NCrontab;
using srtool;

namespace blueprint.modules.schedule.logic
{
    public class ScheduleModule : Module<ScheduleModule>
    {
        public IMongoCollection<database.Schedule> dbContext { get; private set; }
        public event Action<SchedulerResponse> OnAction;

        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<Schedule>("schedule");
            LoopProcess();
            Indexing();
        }
        private async void Indexing()
        {
            try
            {
                var builder = Builders<Schedule>.IndexKeys.Ascending(i => i.updateTime);
                await dbContext.Indexes.CreateOneAsync(new CreateIndexModel<Schedule>(builder, new CreateIndexOptions() { Background = true }));
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
                List<database.Schedule> schedules;
                try
                {
                    schedules = await dbContext
                         .AsQueryable()
                         .Where(i => i.nextOccurrenceTime < DateTime.UtcNow)
                         .Take(50)
                         .ToListAsync();

                    if (schedules.Count > 0)
                    {
                        var ids_to_remove = schedules.Where(i => !i.repeat).Select(i => i._id).ToList();
                        if (ids_to_remove.Count > 0)
                        {
                            await dbContext.DeleteManyAsync(Builders<Schedule>.Filter.In(i => i._id, ids_to_remove));
                        }

                        var now = DateTime.UtcNow;

                        foreach (var schedule in schedules)
                        {
                            #region Renew
                            if (schedule.expression != null)
                            {
                                var cron = CrontabSchedule.Parse(schedule.expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
                                var nextOccurrence = cron.GetNextOccurrence(now);

                                await dbContext.UpdateOneAsync(
                                 Builders<Schedule>.Filter.Eq(i => i.key, schedule.key),
                                   Builders<Schedule>.Update
                                   .Set(i => i.updateTime, now)
                                   .Set(i => i.nextOccurrenceTime, nextOccurrence)
                                 );
                            }
                            #endregion
                            #region Call
                            try
                            {
                                ExucuteSchedule(schedule);
                            }
                            catch (Exception e)
                            {
                                Debug.Error(e);
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.StartsWith("A timeout occured after"))
                        Debug.Warning("[DB] Connection problem.");
                    else
                        Debug.Error(e);
                }

                await Task.Delay(500);
            }
        }
        private void ExucuteSchedule(database.Schedule schedule)
        {
            var s = new SchedulerResponse();
            s.id = schedule._id.ToString();
            s.key = schedule.key;
            s.payload = schedule.payload;
            s.category = schedule.category;
            s.repeat = schedule.repeat;
            s.createDateTime = schedule.nextOccurrenceTime;
            s.invokeTime = schedule.updateTime;

            OnAction?.Invoke(s);
        }
        public async void Upsert(string key, string expression, string payload, string category)
        {
            try
            {
                var now = DateTime.UtcNow;

                var cron = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
                var nextOccurrence = cron.GetNextOccurrence(now);

                await dbContext.UpdateOneAsync(
                Builders<Schedule>.Filter.Eq(i => i.key, key),
                  Builders<Schedule>.Update
                  .Set(i => i.key, key)
                  .Set(i => i.category, category)
                  .Set(i => i.expression, expression)
                  .Set(i => i.payload, payload)
                  .Set(i => i.nextOccurrenceTime, nextOccurrence)
                  .Set(i => i.repeat, true)
                  .Set(i => i.updateTime, now)
                  , new UpdateOptions() { IsUpsert = true }
                );
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async void Upsert(string key, TimeSpan delta, string payload, string category)
        {
            try
            {
                var now = DateTime.UtcNow;

                await dbContext.UpdateOneAsync(
                Builders<Schedule>.Filter.Eq(i => i.key, key),
                  Builders<Schedule>.Update
                  .Set(i => i.key, key)
                  .Set(i => i.category, category)
                  .Set(i => i.expression, null)
                  .Set(i => i.payload, payload)
                  .Set(i => i.nextOccurrenceTime, now + delta)
                  .Set(i => i.repeat, false)
                  .Set(i => i.updateTime, now)
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
