using srtool;

namespace blueprint.modules.blueprint.runtime.tools
{
    public class log
    {
        public void add(object item)
        {
            Debug.Log(item);
        }
    }
    //public async Task delay(double sec, ScriptObject action)
    //{
    //    try
    //    {
    //        await Task.Delay(TimeSpan.FromSeconds(sec));
    //        action.InvokeAsFunction(new object[] { });
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.Error(ex);
    //    }
    //}
}
