using srtool.core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace srtool
{
    public static class SuperQueue
    {
        public static async Task<T> Run<T>(Func<T> task, QueueSetting setting = null)
        {
            var item = GetQueueItem(setting);

            await item.semaphore.WaitAsync();
            try
            {
                return task();
            }
            finally
            {
                item.semaphore.Release();
            }
        }
        public static async Task<T> Run<T>(Func<Task<T>> task, QueueSetting setting = null)
        {
            var item = GetQueueItem(setting);
            await item.semaphore.WaitAsync();
            try
            {

                return await task();
            }
            finally
            {
                item.semaphore.Release();
            }
        }
        public static async Task Run(Func<Task> task, QueueSetting setting = null)
        {
            var item = GetQueueItem(setting);
            await item.semaphore.WaitAsync();
            try
            {
                await task();
            }
            finally
            {
                item.semaphore.Release();
            }
        }

        #region CORE
        private static QueueItem GetQueueItem(QueueSetting setting)
        {
            if (setting == null)
            {
                return SuperCache.Get(() =>
                {
                    return new QueueItem(1);
                },
                new CacheSetting($"queue_DEFAULT____", TimeSpan.FromMinutes(100))
                );
            }
            else
            {
                return SuperCache.Get(() =>
                {
                    return new QueueItem(setting.concurrently);
                },
                new CacheSetting($"queue_{setting.key}_{setting.concurrently}", TimeSpan.FromMinutes(100))
                );
            }
        }
        #endregion
    }
    public class QueueSetting
    {
        public string key;
        public int concurrently = 1;
    }
}
namespace srtool.core
{
    public class QueueItem
    {
        public SemaphoreSlim semaphore;
        public QueueItem(int concorently)
        {
            semaphore = new SemaphoreSlim(concorently);
        }
    }
}