using NCrontab;

namespace blueprint.modules.schedule
{
    public class CronTest
    {
        public void Run()
        {
            var timeSpan = new TimeSpan(1, 0, 0);
            var cron = CrontabSchedule.Parse($"{timeSpan.Seconds} * * * * *");

            var nextTimes = cron.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddDays(100)).Take(100).ToList();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
