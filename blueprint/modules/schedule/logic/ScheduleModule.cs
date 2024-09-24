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
        public List<Schedule> checkinSchedules = new List<Schedule>();
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<Schedule>("schedule");
            //LoopProcess();
            Database_to_checkin();
            Checkin_to_execute();
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

        private async void Database_to_checkin()
        {
            while (true)
            {
                bool wait = false;
                List<database.Schedule> checkin_schedules;
                try
                {
                    var now = DateTime.UtcNow;
                    checkin_schedules = await dbContext
                         .AsQueryable()
                         .Where(i => i.checkinTime < now)
                         .Take(50)
                         .ToListAsync();

                    if (checkin_schedules.Count > 0)
                    {
                        var _ids = checkin_schedules.Select(i => i._id).ToList();
                        await dbContext.UpdateManyAsync(
                            Builders<Schedule>.Filter.In(i => i._id, _ids),
                            Builders<Schedule>.Update.Set(i => i.checkinTime, now.AddSeconds(30)));
                    }
                    else
                    {
                        wait = true;
                    }

                    lock (checkinSchedules)
                    {
                        foreach (var schedule in checkin_schedules)
                        {
                            var item = checkinSchedules.FirstOrDefault(i => i.key == schedule.key);
                            if (item != null)
                                checkinSchedules.Remove(item);

                            checkinSchedules.Add(schedule);
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

                if (wait)
                    await Task.Delay(5000);
            }
        }
        private async void Checkin_to_execute()
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                List<Schedule> items;
                lock (checkinSchedules)
                {
                    items = checkinSchedules.Where(i => i.nextOccurrenceTime < now).ToList();
                    if (items.Count > 0)
                    {
                        foreach (var schedule in items)
                        {
                            if (!schedule.repeat)
                            {
                                remove_by_id(schedule._id);
                            }
                            else
                            {
                                if (schedule.expression != null)
                                {
                                    var cron = CrontabSchedule.Parse(schedule.expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
                                    var nextOccurrence = cron.GetNextOccurrence(now);

                                    schedule.nextOccurrenceTime = nextOccurrence;
                                    schedule.checkinTime = nextOccurrence.AddSeconds(-30);

                                    if (schedule.checkinTime > DateTime.Now)
                                    {
                                        checkinSchedules.Remove(schedule);
                                        InternalUpsertSchedule(schedule.key, schedule);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var item in items)
                {
                    ExucuteSchedule(item);
                }

                await Task.Delay(5);
            }
        }
        //private async void LoopProcess()
        //{
        //    while (true)
        //    {
        //        List<database.Schedule> schedules;
        //        try
        //        {
        //            schedules = await dbContext
        //                 .AsQueryable()
        //                 .Where(i => i.nextOccurrenceTime < DateTime.UtcNow)
        //                 .Take(50)
        //                 .ToListAsync();

        //            if (schedules.Count > 0)
        //            {

        //                var ids_to_remove = schedules.Where(i => !i.repeat).Select(i => i._id).ToList();
        //                if (ids_to_remove.Count > 0)
        //                {
        //                    await dbContext.DeleteManyAsync(Builders<Schedule>.Filter.In(i => i._id, ids_to_remove));
        //                }

        //                var now = DateTime.UtcNow;

        //                foreach (var schedule in schedules)
        //                {
        //                    #region Renew
        //                    if (schedule.expression != null)
        //                    {
        //                        var cron = CrontabSchedule.Parse(schedule.expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
        //                        var nextOccurrence = cron.GetNextOccurrence(now);

        //                        await dbContext.UpdateOneAsync(
        //                         Builders<Schedule>.Filter.Eq(i => i.key, schedule.key),
        //                           Builders<Schedule>.Update
        //                           .Set(i => i.updateTime, now)
        //                           .Set(i => i.nextOccurrenceTime, nextOccurrence)
        //                         );
        //                    }
        //                    #endregion
        //                    #region Call
        //                    try
        //                    {
        //                        ExucuteSchedule(schedule);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Debug.Error(e);
        //                    }
        //                    #endregion
        //                }
        //            }
        //            else
        //            {
        //                await Task.Delay(500);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            if (e.Message.StartsWith("A timeout occured after"))
        //                Debug.Warning("[DB] Connection problem.");
        //            else
        //                Debug.Error(e);
        //        }

        //    }
        //}
        private void ExucuteSchedule(database.Schedule schedule)
        {
            var s = new SchedulerResponse();
            s.key = schedule.key;
            s.expression = schedule.expression;
            s.payload = schedule.payload;
            s.category = schedule.category;
            s.repeat = schedule.repeat;
            s.createDateTime = schedule.nextOccurrenceTime;
            s.invokeTime = schedule.updateTime;

            OnAction?.Invoke(s);
        }
        public void Upsert(string key, string expression, string payload, string category)
        {
            try
            {
                var now = DateTime.UtcNow;

                var cron = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
                var nextOccurrence = cron.GetNextOccurrence(now);

                var schedule = new Schedule();
                schedule.key = key;
                schedule.expression = expression;
                schedule.payload = payload;
                schedule.category = category;
                schedule.nextOccurrenceTime = nextOccurrence;
                schedule.checkinTime = nextOccurrence.AddSeconds(-30);
                schedule.repeat = true;
                schedule.updateTime = now;

                if (schedule.checkinTime < now)
                {
                    var item = checkinSchedules.FirstOrDefault(i => i.key == schedule.key);
                    if (item != null)
                        checkinSchedules.Remove(item);
                    checkinSchedules.Add(schedule);
                }

                InternalUpsertSchedule(key, schedule);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        private async void InternalUpsertSchedule(string key, Schedule schedule)
        {
            try
            {
                await InternalUpsertScheduleAsync(key, schedule);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        private async Task InternalUpsertScheduleAsync(string key, Schedule schedule)
        {
            await dbContext.UpdateOneAsync(
            Builders<Schedule>.Filter.Eq(i => i.key, key),
              Builders<Schedule>.Update
              .Set(i => i.key, schedule.key)
              .Set(i => i.category, schedule.category)
              .Set(i => i.expression, schedule.expression)
              .Set(i => i.payload, schedule.payload)
              .Set(i => i.nextOccurrenceTime, schedule.nextOccurrenceTime)
              .Set(i => i.checkinTime, schedule.checkinTime)
              .Set(i => i.repeat, schedule.repeat)
              .Set(i => i.updateTime, schedule.updateTime)
              , new UpdateOptions() { IsUpsert = true }
            );
        }

        public async void Upsert(string key, TimeSpan delay, string payload, string category)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextOccurrenceTime = now.Add(delay);
                var schedule = new Schedule();
                schedule.key = key;
                schedule.payload = payload;
                schedule.category = category;
                schedule.nextOccurrenceTime = nextOccurrenceTime;
                schedule.checkinTime = nextOccurrenceTime.AddSeconds(-30);
                schedule.repeat = false;
                schedule.updateTime = now;

                if (schedule.checkinTime < now)
                {
                    lock (checkinSchedules)
                    {
                        var item = checkinSchedules.FirstOrDefault(i => i.key == schedule.key);
                        if (item != null)
                            checkinSchedules.Remove(item);
                        checkinSchedules.Add(schedule);
                    }
                }
                else
                {
                    await dbContext.UpdateOneAsync(
                    Builders<Schedule>.Filter.Eq(i => i.key, key),
                      Builders<Schedule>.Update
                      .Set(i => i.key, schedule.key)
                      .Set(i => i.category, schedule.category)
                      .Set(i => i.expression, schedule.expression)
                      .Set(i => i.payload, schedule.payload)
                      .Set(i => i.nextOccurrenceTime, schedule.nextOccurrenceTime)
                      .Set(i => i.checkinTime, schedule.checkinTime)
                      .Set(i => i.repeat, schedule.repeat)
                      .Set(i => i.updateTime, schedule.updateTime)
                      , new UpdateOptions() { IsUpsert = true }
                    );
                }
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        async void remove_by_id(string id)
        {
            try
            {
                await dbContext.DeleteOneAsync(i => i._id == id);
                lock (checkinSchedules)
                {
                    var item = checkinSchedules.FirstOrDefault(i => i._id == id);
                    if (item != null)
                        checkinSchedules.Remove(item);
                }
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
                var item = checkinSchedules.FirstOrDefault(i => i.key == key);
                if (item != null)
                    checkinSchedules.Remove(item);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
    }

}

