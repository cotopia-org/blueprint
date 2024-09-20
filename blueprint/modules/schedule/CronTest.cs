namespace blueprint.modules.schedule
{
    public class CronTest
    {
        public void Run()
        {
            var timeSpan = new TimeSpan(1, 0, 0);
            var cron = Cronos.CronExpression.Parse($"{timeSpan.Seconds} * * * *");

            var nextTimes = cron.GetOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddDays(100)).Take(100).ToList();

            while(true)
            {
                Console.ReadLine();
            }
        }
    }
}
