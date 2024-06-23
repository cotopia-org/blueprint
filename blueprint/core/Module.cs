using MongoDB.Driver;
using srtool;

namespace blueprint.core
{
    public class Module<T> where T : new()
    {
        public static T Instance
        {
            get
            {
                _Instance ??= new T();
                return _Instance;
            }
        }

        private static T _Instance;

        public virtual void Run()
        {
            Debug.System("[Loaded Module: " + GetType().Name + "] ✓");
        }
        public virtual async Task RunAsync()
        {
            Debug.System("[Loaded Module: " + GetType().Name + "] ✓");
            await Task.Yield();
        }
        public virtual void Stop()
        {
            Debug.System("[Stop Module: " + GetType().Name + "] ✓");
        }
        public virtual async Task StopAsync()
        {
            Debug.System("[Stop Module: " + GetType().Name + "] ✓");
            await Task.Yield();
        }
    }
}
